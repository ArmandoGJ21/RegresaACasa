# Regresa a Casa 

Red social para reportar mascotas perdidas: un **muro** de publicaciones, un **cuestionario** para
reportar y un **detalle** con el teléfono del dueño para avisar si alguien vio a la mascota.

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
| [backend/README.md](backend/README.md) | Guía del código del backend, archivo por archivo |
| [docs/mockups-backend-mascotas-perdidas.md](docs/mockups-backend-mascotas-perdidas.md) | Documento de diseño original |

## Estructura del repositorio

```
RegresaACasa/
├── backend/                 API .NET 10 (RegresaACasa.slnx) + Dockerfile
│   ├── src/RegresaACasa.Api/     Controllers · Services · Models · Data · Extensions
│   └── tests/RegresaACasa.Api.Tests/  38 pruebas automáticas
├── mobile/                  App Expo (src/app · models · controllers · views)
├── infra/                   Azure: storage.bicep, deploy-storage.ps1, configure-cors.cs
├── mock/db.json             Datos falsos para json-server
├── docs/                    Documentación
└── docker-compose.yml       PostgreSQL + API (+ Azurite opcional)
```

## Cómo ejecutarlo

### 0. Antes de empezar

| Necesitas | Para qué |
|---|---|
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) **encendido** | Correr la API con PostgreSQL (opción A, recomendada) |
| [.NET SDK 10](https://dotnet.microsoft.com/download) | Correr la API sin Docker (opción B) y las pruebas |
| [Node.js 20+](https://nodejs.org) | La app móvil |
| App **Expo Go** en el celular, o un emulador | Ver la app en el teléfono |

Trae los últimos cambios:

```bash
git pull
```

### 1. Configuración (solo la primera vez)

```bash
cp backend/.env.example backend/.env
cp mobile/.env.example mobile/.env.local
```

- **`backend/.env`:** para subir fotos necesita `AzureBlob__ConnectionString` y
  `AzureBlob__ContainerName`. Pídeselas a Armando **por mensaje privado** o crea tu propio Storage
  ([docs/azure-blob-storage.md](docs/azure-blob-storage.md)). Sin ellas la API funciona igual, pero
  subir fotos responde 503.
- **`mobile/.env.local`:** `EXPO_PUBLIC_API_URL` es la dirección de la API (ver paso 3).
- Ninguno de los dos se sube a git.

### 2. Levantar el backend (elige una opción)

#### Opción A — Docker con PostgreSQL (recomendada)

Desde la raíz del repositorio:

```bash
docker compose up -d --build postgres api
```

- Levanta **PostgreSQL 17** y la **API** en `http://localhost:5105`.
- La primera vez tarda unos minutos (descarga imágenes). La API crea las tablas y carga dos mascotas
  de ejemplo (Max y Luna).
- Los datos se guardan en Docker y **no se borran** al apagar.

| Para… | Comando |
|---|---|
| Ver el log de la API | `docker compose logs -f api` |
| Apagar | `docker compose down` |
| Apagar y **borrar** los datos | `docker compose down -v` |
| Usar otro puerto (ej. 5200) | `API_PORT=5200 docker compose up -d --build postgres api` |
| Aplicar cambios del código | `docker compose up -d --build api` |

#### Opción B — Sin Docker (rápido, datos en memoria)

```bash
cd backend
dotnet run --project src/RegresaACasa.Api --launch-profile http
```

API en `http://localhost:5105`. Los datos se **borran** al detener la API (Ctrl+C).

> Usa solo una opción a la vez: las dos ocupan el puerto 5105.

**Comprueba que funciona:** abre http://localhost:5105/api/v1/pets en el navegador. Debe mostrar
las mascotas en JSON.

### 3. Levantar la app móvil

```bash
cd mobile
npm install
npm start
```

En `mobile/.env.local`, pon en `EXPO_PUBLIC_API_URL` la dirección según dónde abras la app:

| Dónde | `EXPO_PUBLIC_API_URL` | Cómo abrirla |
|---|---|---|
| Navegador | `http://localhost:5105` | Tecla `w` en la terminal de Expo |
| Emulador Android | `http://10.0.2.2:5105` | Tecla `a` |
| Celular (misma Wi-Fi) | `http://<IP-de-tu-PC>:5105` (ver `ipconfig`) | Escanea el QR con Expo Go |

Si cambias `.env.local`, detén Expo (Ctrl+C) y vuelve a correr `npm start`.

### 4. Pruebas

```bash
cd backend
dotnet test
```

```bash
cd mobile
npm run typecheck
```

### Problemas comunes

| Mensaje o síntoma | Causa | Solución |
|---|---|---|
| `error during connect` / `Cannot connect to the Docker daemon` | Docker Desktop apagado | Ábrelo y espera a que diga "Engine running" |
| `port is already allocated` o la API no arranca por el puerto | Otra API ya usa el 5105 | Apaga la otra, o usa `API_PORT=5200` |
| `Falta EXPO_PUBLIC_API_URL` | No existe `mobile/.env.local` o Expo arrancó antes de crearlo | Crea el archivo y reinicia `npm start` |
| `No se pudo conectar con el servidor` | La API no está corriendo o la URL es incorrecta | Revisa el paso 2 y la tabla del paso 3 |
| `Unable to resolve "react-native-web"` | Faltan dependencias | `npm install` dentro de `mobile/` |
| Subir foto responde **503** | Falta la configuración de Azure | Llena `AzureBlob__*` en `backend/.env` |
| `No se pudo subir la foto` solo en el navegador | La cuenta de Azure no tiene CORS | `dotnet run infra/configure-cors.cs` ([detalles](docs/azure-blob-storage.md)) |
| La API se detiene al arrancar con un mensaje de configuración | Una variable del `.env` es inválida | El mensaje dice cuál; reglas en [docs/azure-blob-storage.md](docs/azure-blob-storage.md#4-restricciones-del-env) |

### Mock para el equipo de Frontend

Si el backend no está disponible, `mock/db.json` funciona con json-server (solo lectura del muro):

```bash
npx json-server --watch mock/db.json --port 3000
```

URL: `http://localhost:3000/pets`
