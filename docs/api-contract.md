# Contrato de API — v1

Base URL local: `http://localhost:5105` · JSON en `snake_case` · Sin autenticación (MVP).

Todos los errores usan el formato:

```json
{ "success": false, "message": "Descripción del error" }
```

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/v1/pets` | Muro: publicaciones recientes con comentarios |
| POST | `/api/v1/pets` | Cuestionario: nueva publicación |
| POST | `/api/v1/pets/{id}/comments` | Agrega un comentario |
| POST | `/api/v1/uploads/images` | URL firmada para subir la foto a Azure *(agregado)* |

La especificación OpenAPI se sirve en `GET /openapi/v1.json` (entorno Development).
Hay ejemplos listos para ejecutar en [`backend/src/RegresaACasa.Api/RegresaACasa.Api.http`](../backend/src/RegresaACasa.Api/RegresaACasa.Api.http).

---

## `GET /api/v1/pets`

Devuelve hasta 100 publicaciones ordenadas de la más reciente a la más antigua.
Los comentarios de cada una van en orden cronológico.

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
    "image_url": "https://storage.azure.com/foto.jpg",
    "comments": [
      { "user_name": "Ana", "text": "Lo vi cerca del parque", "created_at": "2026-09-22T02:00:00Z" }
    ]
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
| `image_url` | string (URL http/https) | sí | 500 |

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

**201** — la publicación creada, con `"comments": []`.

**400**
```json
{ "success": false, "message": "El campo 'zone' es requerido" }
```

## `POST /api/v1/pets/{id}/comments`

| Campo | Tipo | Requerido | Máx. |
|---|---|---|---|
| `user_name` | string | sí | 60 |
| `text` | string | sí | 500 |

**Request**
```json
{ "user_name": "Ana", "text": "Lo vi cerca del parque" }
```

**201**
```json
{ "user_name": "Ana", "text": "Lo vi cerca del parque", "created_at": "2026-09-22T02:00:00Z" }
```

**400** — campo faltante (mismo formato que arriba).

**404**
```json
{ "success": false, "message": "No existe una publicación con ese id" }
```

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

**503** — Azure no está configurado (`AzureBlob:ConnectionString` vacío).
