# Azure Blob Storage y variables de entorno

Las fotos de las mascotas se guardan en Azure Blob Storage. La app **nunca** tiene la llave de Azure:
le pide a la API una URL temporal (SAS) de solo escritura, sube la foto con ella y luego manda la
`image_url` en `POST /api/v1/pets` (ver [arquitectura.md](arquitectura.md#2-flujo-de-publicación-foto-primero-luego-json)).

## 1. Qué se crea

Definido en [`infra/storage.bicep`](../infra/storage.bicep):

| Recurso | Configuración | Por qué |
|---|---|---|
| Grupo de recursos `rg-regresaacasa` | región configurable | Agrupa todo para borrarlo fácil |
| Cuenta de almacenamiento `regresaacasa<hash>` | StorageV2, **Standard_LRS**, nivel Hot | La opción más barata; suficiente para el MVP |
| | Solo HTTPS, **TLS 1.2 mínimo** | Nada viaja sin cifrar |
| | Acceso público a blobs **permitido** | Las fotos del muro se ven sin iniciar sesión |
| | Acceso con llave compartida **permitido** | La API firma las URLs SAS con la llave |
| Servicio de blobs | CORS: `GET, PUT, OPTIONS` solo desde los orígenes configurados | Necesario solo para Expo Web; la app nativa no usa CORS |
| | Papelera de blobs: 7 días | Recuperar fotos borradas por error |
| Contenedor `pet-images` | Acceso público nivel **Blob** | Se puede leer cada foto por su URL, pero **no** listar el contenedor |

Restricciones que impone la API al firmar cada URL SAS
([`AzureBlobImageUploadService`](../backend/src/RegresaACasa.Api/Services/AzureBlobImageUploadService.cs)):

- Permisos **Create + Write** sobre **un solo blob**: no permite leer, listar ni borrar otras fotos.
- Vence en `AzureBlob__SasExpiryMinutes` minutos (10 por defecto, máximo 60).
- Nombre aleatorio (`<guid>.jpg`) generado por la API: el cliente no elige la ruta.
- Solo `image/jpeg`, `image/png` o `image/webp`.

> Una SAS no puede limitar el tamaño del archivo. La app comprime la foto (`quality: 0.7`); si hace
> falta un límite estricto, el siguiente paso es validar el tamaño del blob antes de guardar la publicación.

Costo aproximado: Standard_LRS cobra centavos de dólar por GB al mes más las operaciones; para un MVP
escolar es prácticamente cero, y cabe en el crédito de Azure for Students.

## 2. Crear el Storage

### Opción A: script (recomendada)

```powershell
winget install -e --id Microsoft.AzureCLI
```

Cierra y abre la terminal, luego:

```powershell
az login
./infra/deploy-storage.ps1
```

El script:
1. crea el grupo de recursos,
2. despliega `storage.bicep` (cuenta + contenedor + CORS),
3. escribe `AzureBlob__ConnectionString` y `AzureBlob__ContainerName` en `backend/.env`,
   **sin mostrar la llave en pantalla**.

Parámetros útiles:

```powershell
./infra/deploy-storage.ps1 -Location eastus -CorsOrigins 'http://localhost:8081','https://mi-dominio.com'
```

Si Azure responde `RequestDisallowedByAzure` o `RequestDisallowedByPolicy`, tu suscripción (típico en
Azure for Students) no permite esa región; repite con otra `-Location` (`eastus`, `eastus2`, `westus2`...).

### Opción B: portal de Azure (manual)

1. **Crear un recurso → Cuenta de almacenamiento**.
   - Grupo de recursos: `rg-regresaacasa` · Nombre: `regresaacasa` + algo único (solo minúsculas y números).
   - Rendimiento: **Estándar** · Redundancia: **LRS**.
2. Pestaña **Opciones avanzadas**:
   - Requerir transferencia segura: **Sí** · TLS mínimo: **1.2**.
   - **Permitir el acceso anónimo a blobs individuales: Sí** (sin esto las fotos no se ven).
   - Habilitar el acceso a la clave de la cuenta de almacenamiento: **Sí**.
3. Crear y abrir la cuenta → **Contenedores → + Contenedor**.
   - Nombre: `pet-images` · Nivel de acceso: **Blob (acceso de lectura anónimo solo para blobs)**.
4. **Uso compartido de recursos (CORS) → Blob service** (solo si usarás Expo Web):
   orígenes `http://localhost:8081`, métodos `GET, PUT, OPTIONS`, encabezados permitidos `content-type,x-ms-*`,
   encabezados expuestos `etag`, edad máxima `3600`.
5. **Claves de acceso → Mostrar → Cadena de conexión** de `key1` y pégala en `backend/.env`:

   ```
   AzureBlob__ConnectionString=DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net
   ```

### Verificar

```powershell
dotnet run --project backend/src/RegresaACasa.Api --launch-profile http
```

En otra terminal:

```powershell
Invoke-RestMethod -Method Post http://localhost:5105/api/v1/uploads/images -ContentType 'application/json' -Body '{"content_type":"image/jpeg"}'
```

Debe devolver `upload_url`, `image_url` y `expires_at`. Si responde 503, la API no encontró
`AzureBlob__ConnectionString`.

### Borrar todo

```powershell
az group delete --name rg-regresaacasa
```

Esto borra la cuenta y **todas las fotos** de forma permanente.

---

## 3. Restricciones del .env

Hay dos archivos, cada uno con su plantilla. Los archivos reales **están ignorados por git**.

| Archivo real | Plantilla | Lo lee |
|---|---|---|
| `backend/.env` | [`backend/.env.example`](../backend/.env.example) | La API al arrancar ([`DotEnvFile`](../backend/src/RegresaACasa.Api/Configuration/DotEnvFile.cs)) |
| `mobile/.env.local` | [`mobile/.env.example`](../mobile/.env.example) | Expo al empaquetar la app ([`env.ts`](../mobile/src/config/env.ts)) |

Las variables del sistema (por ejemplo, las del servidor de producción) tienen prioridad sobre `backend/.env`.

### Backend (`backend/.env`)

La API valida estas reglas **al arrancar**. Si algo es inválido, no inicia y muestra qué variable está mal.

| Variable | Development | Otros entornos | Reglas |
|---|---|---|---|
| `ConnectionStrings__Default` | Opcional (vacía = BD en memoria) | **Obligatoria** | Cadena de conexión de PostgreSQL |
| `AzureBlob__ConnectionString` | Opcional (vacía = subida responde 503) | **Obligatoria** | Debe incluir `AccountName` y `AccountKey`; fuera de Development: `https` y **sin** Azurite |
| `AzureBlob__ContainerName` | `pet-images` | `pet-images` | 3-63 caracteres, minúsculas/números/guiones, sin `--` ni guion al inicio o final |
| `AzureBlob__SasExpiryMinutes` | `10` | `10` | Entero de 1 a 60 |

Ejemplo del error al arrancar:

```
OptionsValidationException: AzureBlob:ConnectionString debe incluir AccountName y AccountKey para firmar URLs SAS
```

Las pruebas (`dotnet test`) ignoran `backend/.env` y siempre usan la BD en memoria sin Azure.

### App móvil (`mobile/.env.local`)

La app valida al abrir y muestra la pantalla roja de error con el mensaje si algo está mal.

| Variable | Requerida | Reglas |
|---|---|---|
| `EXPO_PUBLIC_API_URL` | **Sí** | URL `http://` o `https://`, sin query; en builds de producción **solo https** |
| `EXPO_PUBLIC_SKIP_IMAGE_UPLOAD` | No | Solo `true` o `false`; `true` **solo en desarrollo** |

**Regla de seguridad:** todo lo que empieza con `EXPO_PUBLIC_` queda incrustado en la app y cualquiera
puede extraerlo. Nunca pongas ahí la llave de Azure, contraseñas ni cadenas de conexión.

### Reglas para el equipo

- Nunca subas `backend/.env` ni `mobile/.env.local`; están en `.gitignore`.
- Si una llave se filtra (commit, captura, chat): **Portal → Cuenta de almacenamiento → Claves de acceso →
  Rotar clave**, y vuelve a correr `./infra/deploy-storage.ps1` para actualizar `backend/.env`.
- Cada integrante tiene su propio `.env`; compartan las plantillas `.env.example`, no los valores.
