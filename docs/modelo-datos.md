# Modelo de datos

PostgreSQL, gestionado con migraciones de EF Core
([`backend/src/RegresaACasa.Api/Data/Migrations`](../backend/src/RegresaACasa.Api/Data/Migrations)).
Sin roles de usuario: todos comparten los mismos permisos.

```mermaid
erDiagram
    PETS ||--o{ COMMENTS : tiene
    PETS {
        uuid id PK
        varchar(30) pet_type "NOT NULL"
        varchar(60) name "NULL"
        varchar(60) breed "NULL"
        varchar(200) color_description "NOT NULL"
        varchar(80) zone "NOT NULL"
        varchar(80) contact_info "NOT NULL"
        varchar(500) image_url "NOT NULL"
        timestamptz created_at "indexado"
    }
    COMMENTS {
        uuid comment_id PK
        uuid pet_id FK "ON DELETE CASCADE"
        varchar(60) user_name "NOT NULL"
        varchar(500) text "NOT NULL"
        timestamptz created_at
    }
```

Notas:
- `comment_id` existe en la BD pero **no** se expone en la API (según el diseño).
- Índice en `pets.created_at` para ordenar el muro.
- Las fotos no se guardan en la BD; solo su `image_url` en Azure Blob Storage.

## Comandos de migraciones

Desde `backend/`:

```bash
dotnet tool restore
dotnet ef migrations add NombreDelCambio -p src/RegresaACasa.Api -o Data/Migrations
```

La API aplica las migraciones pendientes al arrancar cuando usa PostgreSQL.
