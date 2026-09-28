# Backend de Regresa a Casa — Guía del código

API REST en **.NET 10** para una red social de mascotas perdidas. Permite:

- ver el **muro** de publicaciones recientes,
- **publicar** una mascota perdida con su foto,
- obtener una **URL firmada** para subir la foto directo a Azure Blob Storage.

Las fotos viven en un contenedor **privado** de Azure; la base de datos es **PostgreSQL**
(o una base **en memoria** cuando no se configura ninguna, solo en desarrollo).

Esta guía explica cada parte del código: qué hace, para qué sirve, con qué se relaciona y qué
recibe y devuelve. Todo lo descrito existe en el código de esta carpeta.

---

## 1. Cómo correrlo

Requisitos: [.NET SDK 10](https://dotnet.microsoft.com/download).

```bash
dotnet run --project src/RegresaACasa.Api --launch-profile http
```

- La API queda en `http://localhost:5105` (el perfil escucha en `0.0.0.0:5105` para que un celular
  de la misma red pueda llamarla).
- Sin configuración usa la base en memoria y carga dos mascotas de ejemplo (Max y Luna).
- Para PostgreSQL y Azure, copia `.env.example` a `.env` y llena las variables (sección 7).
- Pruebas automáticas:

```bash
dotnet test
```

- Ejemplos listos para ejecutar: `src/RegresaACasa.Api/RegresaACasa.Api.http`.

### Con Docker (PostgreSQL + API)

Requisito: Docker Desktop encendido. Desde la carpeta que tiene `docker-compose.yml`:

```bash
docker compose up -d --build postgres api
```

- Levanta **PostgreSQL 17** (con chequeo de salud) y la **API** en `http://localhost:5105`.
- La API espera a que la base esté lista, aplica las migraciones y carga los datos de ejemplo.
- Toma la llave de Azure de `.env` si existe; la conexión a la base la fija el compose.
- Los datos quedan en el volumen `pgdata`: sobreviven a `docker compose down`
  (`docker compose down -v` los borra).
- Otro puerto: `API_PORT=5200 docker compose up -d --build postgres api`.

---

## 2. Estructura de carpetas

```
backend/
├── RegresaACasa.slnx                  Solución (API + pruebas)
├── dotnet-tools.json                  Herramienta local dotnet-ef (migraciones)
├── .env.example                       Plantilla de variables de entorno
├── Dockerfile / .dockerignore          Imagen de la API (sin copiar .env ni compilados)
├── src/RegresaACasa.Api/
│   ├── Program.cs                     Arranque: carga .env, registra servicios, arma el pipeline
│   ├── Extensions/                    Configuración de arranque agrupada por tema
│   │   ├── ServiceCollectionExtensions.cs   Registro de servicios (AddDatabase, ...)
│   │   └── WebApplicationExtensions.cs      Arranque de la BD y manejo de errores
│   ├── Controllers/                   C de MVC: reciben HTTP y responden
│   │   ├── PetsController.cs                GET y POST /api/v1/pets
│   │   └── UploadsController.cs             POST /api/v1/uploads/images
│   ├── Services/                      Lógica de negocio
│   │   ├── IPetService.cs / PetService.cs   Muro y creación de publicaciones
│   │   └── IImageStorageService.cs / AzureBlobImageStorageService.cs   Fotos en Azure
│   ├── Models/                        M de MVC
│   │   ├── Entities/Pet.cs                  Tabla "pets"
│   │   ├── Dtos/                            Lo que entra y sale en JSON (la "vista" de la API)
│   │   ├── PetMappings.cs                   Entidad → JSON de salida
│   │   └── Result.cs                        Resultado de negocio (valor o error)
│   ├── Data/                          Base de datos
│   │   ├── AppDbContext.cs                  EF Core: tabla y columnas
│   │   ├── DbSeeder.cs                      Datos de ejemplo (Development)
│   │   ├── DesignTimeDbContextFactory.cs    Solo para generar migraciones
│   │   └── Migrations/                      Historial del esquema de PostgreSQL
│   ├── Configuration/                 Opciones y utilidades de configuración
│   │   ├── AzureBlobOptions.cs + AzureBlobOptionsValidator.cs
│   │   ├── RateLimitOptions.cs
│   │   ├── DotEnvFile.cs
│   │   └── SnakeCaseDisplayNameProvider.cs
│   ├── appsettings.json               Valores por defecto
│   └── Properties/launchSettings.json Perfiles de arranque (puerto 5105)
└── tests/RegresaACasa.Api.Tests/      38 pruebas automáticas
```

**Patrón:** MVC en un solo proyecto. El controlador (C) solo traduce HTTP; la lógica está en
`Services/`; los datos y el formato JSON en `Models/` (M), y la "vista" de una API es el JSON que
definen los DTOs.

---

## 3. Cómo viaja una petición

```
Cliente (app móvil)
   │  HTTP + JSON en snake_case
   ▼
Pipeline (Program.cs)
   ├─ UseApiErrorHandler  → si algo explota: 500 { success:false, message }
   ├─ UseCors             → permite llamadas desde el navegador (Expo Web)
   ├─ UseRateLimiter      → si se pasó del límite: 429 { success:false, message }
   ▼
Controlador (Controllers/)
   ├─ [ApiController] valida el DTO de entrada → si falla: 400 { success:false, message }
   ▼
Servicio (Services/)
   ├─ PetService              → reglas de negocio + base de datos (AppDbContext)
   └─ AzureBlobImageStorageService → firma URLs de Azure
   ▼
Respuesta JSON (Models/Dtos) en snake_case
```

### Arranque (Program.cs, en orden)

1. `DotEnvFile.Load(...)` copia las variables de `backend/.env` al entorno.
2. Registra servicios: `AddApiControllers` → `AddDatabase` → `AddAppServices` → `AddRateLimits`.
3. `InitializeDatabaseAsync`: valida la configuración de Azure, aplica migraciones (si es
   PostgreSQL) y en Development carga los datos de ejemplo.
4. Arma el pipeline: manejo de errores, OpenAPI (solo Development) o redirección HTTPS (fuera de
   Development), CORS, límites y controladores.

Si una configuración es inválida, la API **no arranca** y muestra qué variable está mal.

---

## 4. Endpoints

| Método | Ruta | Para qué | Respuestas |
|---|---|---|---|
| GET | `/api/v1/pets` | Muro: publicaciones recientes | 200 |
| POST | `/api/v1/pets` | Crear una publicación (cuestionario) | 201, 400, 429 |
| POST | `/api/v1/uploads/images` | URL firmada para subir una foto | 200, 400, 429, 503 |
| GET | `/openapi/v1.json` | Descripción OpenAPI (solo Development) | 200 |

Todo el JSON usa **snake_case** (`pet_type`, `image_url`...). Los errores siempre tienen la forma:

```json
{ "success": false, "message": "Descripción del error" }
```

Una ruta que no existe responde **404** sin cuerpo.

### GET /api/v1/pets

- **Controlador:** `PetsController.GetAll` → `PetService.GetRecentAsync`.
- **Recibe:** nada.
- **Hace:** lee hasta **100** publicaciones ordenadas por `created_at` de la más nueva a la más
  vieja; a cada `image_url` que sea una foto de nuestro contenedor le agrega una firma de lectura
  válida por 60 minutos (`AzureBlob__ReadSasMinutes`). Las URLs ajenas (como las de ejemplo) salen
  igual.
- **Devuelve 200:**

```json
[
  {
    "id": "ee77dbc7-b074-4543-96f9-a7b82290acf7",
    "pet_type": "Perro",
    "name": "Max",
    "breed": "Labrador",
    "color_description": "Miel con mancha blanca",
    "zone": "Centro",
    "contact_info": "4491234567",
    "image_url": "https://storage.azure.com/foto.jpg"
  }
]
```

### POST /api/v1/pets

- **Controlador:** `PetsController.Create` → `PetService.CreateAsync`.
- **Límite:** 20 por minuto por IP (política `writes`).
- **Recibe** (`CreatePetRequest`):

| Campo | Requerido | Máx. | Regla |
|---|---|---|---|
| `pet_type` | sí | 30 | |
| `name` | no | 60 | |
| `breed` | no | 60 | |
| `color_description` | sí | 200 | |
| `zone` | sí | 80 | |
| `contact_info` | sí | 80 | |
| `image_url` | sí | 500 | URL válida; con Azure configurado, debe ser una foto de **nuestro** contenedor |

```json
{
  "pet_type": "Perro",
  "name": "Max",
  "breed": "Labrador",
  "color_description": "Miel con mancha blanca",
  "zone": "Centro",
  "contact_info": "4491234567",
  "image_url": "https://up23.blob.core.windows.net/up23/0123456789abcdef0123456789abcdef.png"
}
```

- **Hace:** recorta espacios de los textos, genera el `id` (UUID) y la fecha `created_at`, guarda
  en la base y devuelve la publicación con la foto ya firmada.
- **Devuelve:**
  - **201** con la publicación (misma forma que un elemento del muro).
  - **400** `"El campo 'zone' es requerido"` (u otro campo) si falta un dato o se pasa del máximo.
  - **400** `"El campo 'image_url' debe ser una URL válida"`.
  - **400** `"El campo 'image_url' debe ser una foto subida con /api/v1/uploads/images"` si la
    foto no es de nuestro contenedor.
  - **429** si se superó el límite.

### POST /api/v1/uploads/images

- **Controlador:** `UploadsController.CreateImageUpload` → `AzureBlobImageStorageService.CreateUploadUrlAsync`.
- **Límite:** 5 por minuto por IP y **200 al día en total** (todas las IPs).
- **Recibe:** `{ "content_type": "image/jpeg" }` — solo `image/jpeg`, `image/png` o `image/webp`.
- **Hace:** crea el contenedor si no existe (privado), elige un nombre aleatorio
  `<guid>.<ext>` y firma una URL que solo permite **crear/escribir ese archivo** durante 10 minutos
  (`AzureBlob__SasExpiryMinutes`).
- **Devuelve:**
  - **200**:

    ```json
    {
      "upload_url": "https://up23.blob.core.windows.net/up23/<nombre>.png?sv=...&sig=...",
      "image_url": "https://up23.blob.core.windows.net/up23/<nombre>.png",
      "expires_at": "2026-09-27T20:10:00+00:00"
    }
    ```

  - **400** `"El campo 'content_type' debe ser image/jpeg, image/png o image/webp"`.
  - **429** si se superó el límite.
  - **503** `"El almacenamiento de imágenes no está configurado"` si falta
    `AzureBlob__ConnectionString`.

### Flujo completo para publicar con foto

```
1. App → POST /api/v1/uploads/images { content_type }        → recibe upload_url + image_url
2. App → PUT upload_url (bytes de la foto)                     → directo a Azure (no pasa por la API)
         encabezados: x-ms-blob-type: BlockBlob, Content-Type: <el mismo content_type>
3. App → POST /api/v1/pets { ...datos, image_url }             → 201 con la publicación
4. App → GET /api/v1/pets                                      → la foto llega con firma de lectura
```

La foto nunca pasa por la API y la app nunca recibe la llave de Azure.

---

## 5. Archivo por archivo

### Program.cs
- **Qué hace:** arranca la aplicación en 4 pasos (sección 3).
- **Relación:** llama a los métodos de `Extensions/`; declara `public partial class Program` para
  que las pruebas puedan levantar la API en memoria.

### Extensions/ServiceCollectionExtensions.cs
Registra servicios en el contenedor de dependencias, agrupados por tema:

| Método | Qué configura |
|---|---|
| `AddApiControllers()` | Controladores; JSON en snake_case; nombres JSON en los mensajes de validación; la respuesta 400 `{ success:false, message }` con el **primer** error encontrado; OpenAPI; CORS abierto |
| `AddDatabase(config, env)` | `AppDbContext` con PostgreSQL (con reintentos ante fallas transitorias) si hay `ConnectionStrings:Default`; si no, base en memoria. Fuera de Development sin conexión: **error al arrancar** |
| `AddAppServices(config)` | `TimeProvider` (reloj), `PetService`, opciones de Azure con validación al arrancar y `AzureBlobImageStorageService` |
| `AddRateLimits(config)` | Límites: tope diario global en `/api/v1/uploads`, políticas `uploads` y `writes` por IP y por minuto; respuesta 429 |

### Extensions/WebApplicationExtensions.cs
- `InitializeDatabaseAsync()`: fuerza la validación de `AzureBlobOptions` **antes** de tocar la
  base; si la base es PostgreSQL aplica migraciones pendientes; en Development llama a `DbSeeder`.
- `UseApiErrorHandler()`: cualquier excepción no controlada se registra en el log y responde
  `500 { "success": false, "message": "Ocurrió un error inesperado" }`.

### Controllers/PetsController.cs
- **Qué hace:** atiende `/api/v1/pets`. `GetAll` devuelve el muro; `Create` pide a `PetService`
  crear la publicación y responde 201 o 400 según el `Result`.
- **No contiene lógica de negocio:** solo traduce HTTP ⇄ servicio.
- **Atributos:** `[ApiController]` valida el DTO automáticamente; `[EnableRateLimiting("writes")]`
  aplica el límite al POST.

### Controllers/UploadsController.cs
- **Qué hace:** atiende `POST /api/v1/uploads/images`. Si Azure no está configurado responde 503;
  si sí, devuelve la URL firmada.
- **Límite:** `[EnableRateLimiting("uploads")]` en toda la clase.

### Services/IPetService.cs y PetService.cs
- **Qué hace:** la lógica de las publicaciones.
  - `GetRecentAsync`: consulta la tabla `pets` (sin seguimiento de cambios, más ligero), ordena,
    toma 100 (`FeedSize`) y convierte cada una a `PetResponse` con la foto firmada.
  - `CreateAsync`: aplica la regla "solo fotos propias" (`IsAcceptedImageUrl`); si no se cumple
    devuelve `Result.Failure(ForeignImageError)` sin guardar. Si se cumple, crea el `Pet`,
    lo guarda y devuelve `Result.Success(...)`.
- **Depende de:** `AppDbContext` (base), `IImageStorageService` (fotos), `TimeProvider` (hora).

### Services/IImageStorageService.cs y AzureBlobImageStorageService.cs
- **Qué hace:** todo lo relacionado con fotos en Azure Blob Storage.

| Miembro | Qué hace |
|---|---|
| `IsConfigured` | `true` si hay `AzureBlob__ConnectionString` |
| `CreateUploadUrlAsync(contentType)` | Crea el contenedor si falta (solo la primera vez), nombre aleatorio, SAS de Create+Write con el `Content-Type` fijado |
| `IsAcceptedImageUrl(url)` | Sin Azure: acepta todo. Con Azure: solo URLs de nuestro servidor y contenedor, sin query y con nombre `<32 hex>.jpg/png/webp` |
| `ToReadUrl(url)` | Si la foto es nuestra, le agrega una SAS de solo lectura; si no, la devuelve igual |

- Firmar una URL es un cálculo **local** (no llama a Azure), por eso firmar todo el muro es rápido.
- Se registra como **singleton**: el cliente de Azure se crea una sola vez en el constructor, así
  que varias peticiones simultáneas no comparten estado que cambie.

### Models/Entities/Pet.cs
- **Qué es:** una fila de la tabla `pets`: `Id`, `PetType`, `Name`, `Breed`, `ColorDescription`,
  `Zone`, `ContactInfo`, `ImageUrl` (sin firma) y `CreatedAt`.

### Models/Dtos/
| Archivo | Contenido |
|---|---|
| `PetDtos.cs` | `PetResponse` (salida del muro y del POST) y `CreatePetRequest` (entrada del POST, con sus reglas `[Required]`, `[StringLength]`, `[Url]`) |
| `UploadDtos.cs` | `CreateImageUploadRequest` (`content_type` con expresión regular) y `ImageUploadResponse` |
| `ErrorResponse.cs` | El formato de error `{ success:false, message }`; `success` siempre va primero |
| `ValidationMessages.cs` | El texto compartido `"El campo '{0}' es requerido"` |

### Models/PetMappings.cs
- `ToResponse(pet, toImageUrl)`: convierte la entidad en `PetResponse`. Recibe la función que
  transforma la URL de la foto (en la práctica, `ToReadUrl`).

### Models/Result.cs
- `Result<T>`: el resultado de una operación de negocio. `IsSuccess` + `Value`, o `Error` con el
  mensaje que el controlador devuelve como 400. Evita usar excepciones para errores esperados.

### Data/AppDbContext.cs
- **Qué hace:** conexión con la base mediante Entity Framework Core. Define la tabla `pets`, sus
  columnas en snake_case, longitudes máximas, campos obligatorios y un índice en `created_at` para
  ordenar el muro rápido.

### Data/DbSeeder.cs
- Si la tabla está vacía, inserta **Max** (Perro, Labrador, Centro) y **Luna** (Gato, Siamés,
  Pulgas). Solo se usa en Development.

### Data/DesignTimeDbContextFactory.cs
- Solo lo usa `dotnet ef` para generar migraciones de PostgreSQL, aunque la API esté en modo
  memoria. Toma `ConnectionStrings__Default` o una conexión local por defecto.

### Data/Migrations/
| Migración | Cambio |
|---|---|
| `InitialCreate` | Crea `pets` y `comments` |
| `RemoveComments` | Borra `comments` (la función de comentarios se eliminó) |

### Configuration/
| Archivo | Qué hace |
|---|---|
| `AzureBlobOptions.cs` | Variables `AzureBlob__*` con sus reglas (nombre de contenedor, 1-60 min de subida, 5-1440 min de lectura) |
| `AzureBlobOptionsValidator.cs` | Reglas que dependen del entorno: conexión obligatoria fuera de Development, con `AccountName` y `AccountKey`, `https` y sin Azurite fuera de Development |
| `RateLimitOptions.cs` | Variables `RateLimits__*` y los nombres de política `uploads` y `writes` |
| `DotEnvFile.cs` | Lee `.env` (formato `CLAVE=VALOR`) buscando hacia arriba hasta la carpeta con `RegresaACasa.slnx`; no pisa variables que ya existan |
| `SnakeCaseDisplayNameProvider.cs` | Hace que `{0}` en los mensajes sea `zone` y no `Zone` |

---

## 6. Validaciones y reglas

| Tipo | Regla | Dónde está |
|---|---|---|
| Datos de entrada | Campos requeridos y longitudes de `CreatePetRequest` | `Models/Dtos/PetDtos.cs` |
| Datos de entrada | `content_type` solo jpeg/png/webp | `Models/Dtos/UploadDtos.cs` |
| Negocio | Solo fotos subidas a nuestro contenedor | `PetService.CreateAsync` + `IsAcceptedImageUrl` |
| Negocio | El muro devuelve máximo 100, más nuevas primero | `PetService.GetRecentAsync` |
| Seguridad | Fotos privadas: se leen solo con URL firmada temporal | `AzureBlobImageStorageService.ToReadUrl` |
| Seguridad | URL de subida: un solo archivo, solo escritura, vence en 10 min | `CreateUploadUrlAsync` |
| Anti-abuso | 5 subidas/min por IP, 200 subidas/día en total, 20 publicaciones/min por IP | `AddRateLimits` |
| Arranque | Configuración inválida = la API no inicia | `AzureBlobOptions*`, `AddDatabase`, `AddRateLimits` |

---

## 7. Configuración (`.env`)

`.env` vive junto a `RegresaACasa.slnx` y **no debe subirse a git** (tiene la llave de Azure).
`__` equivale a `:` de `appsettings.json`.

| Variable | Por defecto | Regla |
|---|---|---|
| `ConnectionStrings__Default` | vacía = base en memoria | Obligatoria fuera de Development |
| `AzureBlob__ConnectionString` | vacía = subida responde 503 | Con `AccountName` y `AccountKey`; obligatoria fuera de Development |
| `AzureBlob__ContainerName` | `pet-images` | 3-63: minúsculas, números, guiones |
| `AzureBlob__SasExpiryMinutes` | 10 | 1-60 |
| `AzureBlob__ReadSasMinutes` | 60 | 5-1440 |
| `RateLimits__UploadsPerMinutePerIp` | 5 | 1-1000 |
| `RateLimits__UploadsPerDay` | 200 | 1-100000 |
| `RateLimits__WritesPerMinutePerIp` | 20 | 1-1000 |

Migraciones nuevas (desde esta carpeta):

```bash
dotnet tool restore
dotnet ef migrations add NombreDelCambio -p src/RegresaACasa.Api -o Data/Migrations
```

---

## 8. Pruebas (`tests/RegresaACasa.Api.Tests`)

| Archivo | Qué comprueba | Casos |
|---|---|---|
| `PetsApiTests.cs` | Endpoints reales en memoria: snake_case, 201, error de `zone`, 503 sin Azure | 4 |
| `PetServiceTests.cs` | Regla de fotos propias, recorte de espacios, orden del muro | 3 |
| `RateLimitTests.cs` | 429 en subidas y en publicaciones | 2 |
| `AzureBlobImageStorageServiceTests.cs` | Qué URLs se aceptan y cómo se firman | 10 |
| `AzureBlobOptionsValidatorTests.cs` | Reglas de la conexión, contenedor y minutos | 19 |

`ApiFactory.cs` levanta la API para las pruebas con base en memoria y **sin** Azure, aunque exista
un `.env`.

---

## 9. Estabilidad y operación

Decisiones que mantienen la API estable, verificadas en Docker con PostgreSQL:

| Situación | Qué hace la API | Dónde |
|---|---|---|
| PostgreSQL tarda en arrancar o se corta la conexión | Reintenta automáticamente errores transitorios (`EnableRetryOnFailure`) | `AddDatabase` |
| Varias peticiones al mismo tiempo | `AzureBlobImageStorageService` es una sola instancia sin estado cambiante: el cliente de Azure se crea una vez en el constructor | `AzureBlobImageStorageService` |
| Configuración inválida | La API no arranca y dice qué variable está mal | `InitializeDatabaseAsync`, `AddDatabase`, `AddRateLimits` |
| Error inesperado | Se registra en el log y responde 500 con el formato del contrato | `UseApiErrorHandler` |
| Reinicio de la API | Los datos siguen en PostgreSQL; los de ejemplo solo se cargan si la tabla está vacía | `DbSeeder` |

Notas:

- La primera vez contra una base vacía, EF Core escribe en el log un `fail` al consultar
  `__EFMigrationsHistory`: es normal (la tabla aún no existe) y enseguida la crea.
- Los límites por IP usan la IP que ve la API. Detrás de Docker o de un proxy, varias personas
  pueden compartir esa IP; en ese caso los límites por IP actúan como un límite común.
- Los contadores de límites viven en memoria: se reinician al reiniciar la API.

---

## 10. Glosario

- **DTO:** clase que define exactamente qué JSON entra o sale.
- **EF Core:** librería que traduce clases de C# a tablas de la base de datos.
- **Migración:** archivo que describe un cambio del esquema de la base; con PostgreSQL la API aplica
  las pendientes automáticamente al arrancar.
- **SAS (Shared Access Signature):** firma que se agrega a una URL de Azure para permitir una acción
  concreta (leer o escribir un archivo) por tiempo limitado.
- **snake_case:** nombres en minúsculas separados por guion bajo (`pet_type`).
- **Rate limiting:** límite de peticiones por tiempo para evitar abusos.
