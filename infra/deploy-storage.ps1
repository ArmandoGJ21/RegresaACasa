<#
.SYNOPSIS
    Crea el Azure Blob Storage de Regresa a Casa y escribe la cadena de conexión en backend/.env.

.DESCRIPTION
    Requisitos: Azure CLI (winget install -e --id Microsoft.AzureCLI) y haber iniciado sesión con "az login".
    Es idempotente: puedes correrlo varias veces sin duplicar recursos.

.EXAMPLE
    ./infra/deploy-storage.ps1
    ./infra/deploy-storage.ps1 -ResourceGroup rg-regresaacasa -Location eastus
#>
param(
    [string]$ResourceGroup = 'rg-regresaacasa',
    [string]$Location = 'centralus',
    [ValidatePattern('^(?!.*--)[a-z0-9][a-z0-9-]{1,61}[a-z0-9]$')]
    [string]$ContainerName = 'pet-images',
    [string[]]$CorsOrigins = @('http://localhost:8081'),
    [switch]$SkipEnvFile
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$envFile = Join-Path $repoRoot 'backend/.env'

function Invoke-Az {
    $output = & az @args
    if ($LASTEXITCODE -ne 0) { throw "Falló: az $($args -join ' ')" }
    return $output
}

if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    throw 'No se encontró Azure CLI. Instálalo con: winget install -e --id Microsoft.AzureCLI'
}
# En PowerShell 5.1, redirigir stderr de un .exe con 'Stop' lanza error; se relaja solo aquí.
$ErrorActionPreference = 'Continue'
& az account show --output none 2>$null
$loggedIn = $LASTEXITCODE -eq 0
$ErrorActionPreference = 'Stop'
if (-not $loggedIn) {
    throw 'No has iniciado sesión en Azure. Ejecuta primero: az login'
}

$subscription = Invoke-Az account show --query name --output tsv
Write-Host "Suscripción: $subscription" -ForegroundColor Cyan

Write-Host "1/3 Grupo de recursos '$ResourceGroup' en '$Location'..."
Invoke-Az group create --name $ResourceGroup --location $Location --output none

Write-Host '2/3 Desplegando storage.bicep (cuenta + contenedor + CORS)...'
$parameters = @{
    '$schema'      = 'https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#'
    contentVersion = '1.0.0.0'
    parameters     = @{
        containerName      = @{ value = $ContainerName }
        corsAllowedOrigins = @{ value = @($CorsOrigins) }
    }
}
$parametersFile = Join-Path ([IO.Path]::GetTempPath()) "regresaacasa-storage-params.json"
[IO.File]::WriteAllText($parametersFile, (ConvertTo-Json $parameters -Depth 6))

try {
    $outputs = Invoke-Az deployment group create `
        --resource-group $ResourceGroup `
        --name regresaacasa-storage `
        --template-file (Join-Path $PSScriptRoot 'storage.bicep') `
        --parameters "@$parametersFile" `
        --query properties.outputs `
        --output json | Out-String | ConvertFrom-Json
}
finally {
    Remove-Item $parametersFile -ErrorAction SilentlyContinue
}

$accountName = $outputs.storageAccountName.value
Write-Host "   Cuenta: $accountName"
Write-Host "   Contenedor: $($outputs.containerUrl.value)"

if ($SkipEnvFile) {
    Write-Host '3/3 Omitido (-SkipEnvFile). Obtén la cadena con:'
    Write-Host "   az storage account show-connection-string -g $ResourceGroup -n $accountName --query connectionString -o tsv"
    return
}

Write-Host '3/3 Guardando la cadena de conexión en backend/.env (ignorado por git)...'
$connectionString = Invoke-Az storage account show-connection-string `
    --resource-group $ResourceGroup --name $accountName --query connectionString --output tsv

$managedKeys = 'AzureBlob__ConnectionString', 'AzureBlob__ContainerName'
$lines = @()
if (Test-Path $envFile) {
    $lines = Get-Content $envFile | Where-Object {
        $key = ($_ -split '=', 2)[0].Trim()
        $managedKeys -notcontains $key
    }
}
$lines += "AzureBlob__ConnectionString=$connectionString"
$lines += "AzureBlob__ContainerName=$ContainerName"
[IO.File]::WriteAllLines($envFile, [string[]]$lines)

Write-Host "Listo. Variables escritas en $envFile" -ForegroundColor Green
Write-Host 'La llave NO se muestra en pantalla. No compartas ni subas backend/.env.'
