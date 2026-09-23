# Contrato de API — v1

Base URL local: `http://localhost:5105` · JSON en `snake_case` · Sin autenticación (MVP).

Todos los errores usan el formato:

```json
{ "success": false, "message": "Descripción del error" }
```

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/v1/pets` | Muro: publicaciones recientes |
| POST | `/api/v1/pets` | Cuestionario: nueva publicación |
| POST | `/api/v1/uploads/images` | URL firmada para subir la foto a Azure *(agregado)* |

La especificación OpenAPI se sirve en `GET /openapi/v1.json` (entorno Development).
Hay ejemplos listos para ejecutar en [`backend/src/RegresaACasa.Api/RegresaACasa.Api.http`](../backend/src/RegresaACasa.Api/RegresaACasa.Api.http).

---

### Límites anti-abuso

| Endpoint | Límite | Al superarlo |
|---|---|---|
| `POST /api/v1/uploads/images` | 5 por minuto por IP y 200 al día en total | **429** |
| `POST /api/v1/pets` | 20 por minuto por IP | **429** |

```json
{ "success": false, "message": "Demasiadas solicitudes, intenta de nuevo más tarde" }
```

Configurables con `RateLimits__*` (ver [azure-blob-storage.md](azure-blob-storage.md#4-restricciones-del-env)).

---

## `GET /api/v1/pets`

Devuelve hasta 100 publicaciones ordenadas de la más reciente a la más antigua.

Las fotos están en un contenedor **privado**: `image_url` llega con una firma de lectura (SAS) válida
por 60 minutos. La app debe volver a pedir el muro para obtener URLs nuevas; no debe guardarlas.

**200**
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

## `POST /api/v1/pets`

La foto ya debe estar subida (ver `uploads/images`).

| Campo | Tipo | Requerido | Máx. |
|---|---|---|---|
| `pet_type` | string | sí | 30 |
| `name` | string | no | 60 |
| `breed` | string | no | 60 |
| `color_description` | string | sí | 200 |
| `zone` | string | sí | 80 |
| `contact_info` | string | sí | 80 |
| `image_url` | string: el `image_url` que devolvió `uploads/images` | sí | 500 |

**Request**
```json
{
  "pet_type": "Perro",
  "name": "Max",
  "breed": "Labrador",
  "color_description": "Miel con mancha blanca",
  "zone": "Centro",
  "contact_info": "4491234567",
  "image_url": "https://storage.azure.com/foto.jpg"
}
```

**201**: la publicación creada, con `image_url` ya firmada para lectura.

**400**
```json
{ "success": false, "message": "El campo 'zone' es requerido" }
```
```json
{ "success": false, "message": "El campo 'image_url' debe ser una foto subida con /api/v1/uploads/images" }
```
(Con Azure configurado, solo se aceptan fotos de nuestro contenedor; en desarrollo sin Azure se acepta cualquier URL.)

## `POST /api/v1/uploads/images` *(agregado)*

Genera una URL SAS de solo escritura, válida 10 minutos, para subir **una** foto directo a Azure.

**Request**
```json
{ "content_type": "image/jpeg" }
```
`content_type` admite `image/jpeg`, `image/png` o `image/webp`.

**200**
```json
{
  "upload_url": "https://<cuenta>.blob.core.windows.net/pet-images/3f2a...jpg?sv=...&sig=...",
  "image_url": "https://<cuenta>.blob.core.windows.net/pet-images/3f2a...jpg",
  "expires_at": "2026-09-22T02:10:00+00:00"
}
```

Después la app hace:

```
PUT {upload_url}
x-ms-blob-type: BlockBlob
Content-Type: image/jpeg
<bytes de la foto>
```

y envía `image_url` en `POST /api/v1/pets`.

**429**: límite superado (ver arriba).

**503**: Azure no está configurado (`AzureBlob:ConnectionString` vacío).
