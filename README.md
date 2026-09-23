# Regresa a Casa 

Red social para reportar mascotas perdidas: un **muro** de publicaciones, un **cuestionario** para
reportar y **comentarios** para avisar si alguien vio a la mascota.

- **App móvil:** React Native + Expo + TypeScript (patrón MVC)
- **Backend:** ASP.NET Core **.NET 10** Web API (patrón MVC)
- **Base de datos:** PostgreSQL (EF Core) · **Fotos:** Azure Blob Storage

## Documentación

| Documento | Contenido |
|---|---|
| [docs/arquitectura.md](docs/arquitectura.md) | Componentes, flujo de datos, MVC en backend y móvil, decisiones |
| [docs/api-contract.md](docs/api-contract.md) | Endpoints, requests/responses y errores |
| [docs/modelo-datos.md](docs/modelo-datos.md) | Esquema ER y migraciones |
| [docs/azure-blob-storage.md](docs/azure-blob-storage.md) | Crear el Azure Blob Storage y restricciones del `.env` |
| [docs/mockups-backend-mascotas-perdidas.md](docs/mockups-backend-mascotas-perdidas.md) | Documento de diseño original |

## Estructura del repositorio

```
RegresaACasa/
├── backend/                 Solución .NET 10 (RegresaACasa.slnx)
│   ├── src/RegresaACasa.Api/     Controllers · Models · Services · Data
│   └── tests/RegresaACasa.Api.Tests/  Pruebas de integración del contrato
├── mobile/                  App Expo (src/app · models · controllers · views)
├── mock/db.json             Datos falsos para json-server
├── infra/                   Azure: storage.bicep + deploy-storage.ps1
├── docs/                    Documentación
└── docker-compose.yml       PostgreSQL + Azurite para desarrollo
```

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org) y la app **Expo Go** en el celular (o un emulador)
- Opcional: Docker (para PostgreSQL y Azurite)
- Opcional: [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli-windows) para crear el Blob Storage

## Cómo correrlo

### 1. Backend (modo rápido, BD en memoria)

```bash
cd backend
dotnet run --project src/RegresaACasa.Api --launch-profile http
```

La API queda en `http://localhost:5105` con datos de ejemplo. Prueba:
`GET http://localhost:5105/api/v1/pets`.

### 2. Backend con PostgreSQL y Azure Blob Storage

La configuración va en `backend/.env` (ignorado por git). Parte de la plantilla:

```bash
cp backend/.env.example backend/.env
```

- **PostgreSQL local:** `docker compose up -d` y descomenta `ConnectionStrings__Default`.
  Las migraciones se aplican solas al arrancar.
- **Fotos en Azure real:** `az login` y luego `./infra/deploy-storage.ps1`; crea el Storage y escribe
  la cadena de conexión en `backend/.env`. Guía completa: [docs/azure-blob-storage.md](docs/azure-blob-storage.md).
- **Fotos en local (Azurite):** `AzureBlob__ConnectionString=UseDevelopmentStorage=true`.

```bash
cd backend
dotnet run --project src/RegresaACasa.Api --launch-profile http
```

La API valida las variables al arrancar y no inicia si alguna es inválida
([restricciones del .env](docs/azure-blob-storage.md#3-restricciones-del-env)).

> Con Azurite, las URLs apuntan a `127.0.0.1`, que un celular físico no alcanza. Para probar fotos en
> un dispositivo usa una cuenta real de Azure o `EXPO_PUBLIC_SKIP_IMAGE_UPLOAD=true`.

### 3. App móvil

```bash
cd mobile
npm install
cp .env.example .env.local
npm start
```

Ajusta `EXPO_PUBLIC_API_URL` en `.env.local` (obligatoria; la app valida el formato al abrir):
- Emulador Android: `http://10.0.2.2:5105`
- Celular físico (misma red Wi-Fi): `http://<IP-de-tu-PC>:5105`

Escanea el QR con Expo Go.

### Pruebas y verificación

```bash
cd backend
dotnet test
```

```bash
cd mobile
npm run typecheck
```

### Mock para el equipo de Frontend

Si el backend no está listo, `mock/db.json` funciona con json-server (solo lectura del muro):

```bash
npx json-server --watch mock/db.json --port 3000
```

URL: `http://localhost:3000/pets`
