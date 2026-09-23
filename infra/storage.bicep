// Azure Blob Storage para las fotos de Regresa a Casa.
// Crea: cuenta de almacenamiento + servicio de blobs (CORS, papelera) + contenedor PRIVADO.
// Nadie lee ni escribe fotos sin una URL firmada (SAS) que emite la API.
// Despliegue: infra/deploy-storage.ps1 (o ver docs/azure-blob-storage.md).

@description('Región de Azure. Con Azure for Students usa una región permitida por tu suscripción.')
param location string = resourceGroup().location

@description('Nombre global único: 3-24 caracteres, solo minúsculas y números.')
@minLength(3)
@maxLength(24)
param storageAccountName string = take('regresaacasa${uniqueString(resourceGroup().id)}', 24)

@description('Contenedor de fotos. Debe coincidir con AzureBlob__ContainerName.')
@minLength(3)
@maxLength(63)
param containerName string = 'pet-images'

@description('Orígenes web que pueden subir fotos con la URL SAS (Expo Web). La app nativa no usa CORS.')
param corsAllowedOrigins array = [
  'http://localhost:8081'
]

@description('Días que se conservan los blobs borrados antes de eliminarse definitivamente.')
@minValue(1)
@maxValue(365)
param deleteRetentionDays int = 7

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS' // La opción más barata; suficiente para un MVP.
  }
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false // Sin acceso anónimo: las fotos se leen con URL firmada temporal.
    allowSharedKeyAccess: true // La API firma las URLs SAS con la llave (que solo vive en backend/.env).
    publicNetworkAccess: 'Enabled'
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storage
  name: 'default'
  properties: {
    cors: {
      corsRules: [
        {
          allowedOrigins: corsAllowedOrigins
          allowedMethods: [
            'GET'
            'PUT'
            'OPTIONS'
          ]
          allowedHeaders: [
            'content-type'
            'x-ms-*'
          ]
          exposedHeaders: [
            'etag'
          ]
          maxAgeInSeconds: 3600
        }
      ]
    }
    deleteRetentionPolicy: {
      enabled: true
      days: deleteRetentionDays
    }
  }
}

resource container 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: containerName
  properties: {
    publicAccess: 'None' // Privado: ni lectura ni listado anónimos.
  }
}

output storageAccountName string = storage.name
output blobEndpoint string = storage.properties.primaryEndpoints.blob
output containerUrl string = '${storage.properties.primaryEndpoints.blob}${container.name}'
