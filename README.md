# Álbum Mundial 2026 Tracker

Aplicación full stack local para gestionar el progreso de las láminas del álbum Panini del Mundial 2026. La solución está separada en frontend Angular y backend .NET 8 Web API, desacoplados exclusivamente por API REST.

## Stack

- Frontend: Angular standalone SPA
- Backend: ASP.NET Core Web API .NET 8
- Base de datos: SQLite
- Licencia: MIT

## Arquitectura

- `src/backend/Panini2026Tracker.Api`: capa de presentación REST
- `src/backend/Panini2026Tracker.Application`: casos de uso y contratos
- `src/backend/Panini2026Tracker.Domain`: entidades y abstracciones del dominio
- `src/backend/Panini2026Tracker.Infrastructure`: EF Core, SQLite, seed, repositorios y almacenamiento local
- `src/frontend`: SPA Angular por features
- `src/launcher/Panini2026Tracker.Launcher`: ejecutable lanzador para distribución local
- `src/backend/Panini2026Tracker.Api/Seed/album-catalog.json`: catálogo inicial editable del álbum
- `app-config.json`: configuración centralizada de metadata mostrada en el footer y leída por backend/frontend

## Módulos disponibles

- Álbum segmentado por país con filtros globales
- Modal de detalle de lámina
- Gestión de láminas repetidas
- Ventana de logs con eliminación confirmada
- Ventana de carga y asociación de imágenes
- Modo oscuro

## Requisitos previos

- .NET SDK 8 o superior
- Node.js 18.19 o superior
- npm

## Ejecución local

### 1. Restaurar dependencias backend

```powershell
dotnet restore .\Panini2026Tracker.sln
```

### 2. Ejecutar backend

```powershell
dotnet run --project .\src\backend\Panini2026Tracker.Api
```

Backend esperado:

- `http://localhost:5098`
- Swagger en `http://localhost:5098/swagger`

### 3. Instalar dependencias frontend

```powershell
cd .\src\frontend
npm install
```

### 4. Ejecutar frontend

```powershell
npm start
```

Frontend esperado:

- `http://localhost:4200`

## Distribución local

### Build Release

```powershell
dotnet build .\src\backend\Panini2026Tracker.Api\Panini2026Tracker.Api.csproj -c Release
```

Ese build deja la salida lista en `app-runtime` con esta estructura:

```text
app-runtime/
  AlbumMundial2026_Tracker.exe
  app/
    Panini2026Tracker.Api.exe
    app-config.json
    appsettings.json
    wwwroot/
    ...
```

Comportamiento del ejecutable:

- `AlbumMundial2026_Tracker.exe` es el punto de entrada para distribución.
- El launcher inicia el backend dentro de `app/`.
- El backend sirve el frontend compilado desde `app/wwwroot`.
- Al abrirse en modo distribuido, la aplicación lanza el navegador automáticamente en una URL local.
- Cuando la sesión del navegador se cierra, el backend también se detiene.

### Empaquetado ZIP

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\package-release.ps1
```

El script:

- lee la versión desde `app-config.json` en `AppMeta.version`
- crea la carpeta `release` si no existe
- genera un ZIP con nombre `AlbumMundial2026_Tracker_v{version}.zip`

Estructura del ZIP:

```text
AlbumMundial2026_Tracker_v{version}/
  AlbumMundial2026_Tracker.exe
  app/
    ...
```

## Catálogo inicial

El catálogo inicial no está hardcodeado en la base de datos como fuente principal. Se carga desde [album-catalog.json](./src/backend/Panini2026Tracker.Api/Seed/album-catalog.json), organizado por selecciones y con identificadores provisionales editables.

Campos incluidos por lámina:

- identificador provisional
- país
- tipo
- referencia de imagen
- información adicional
- metadata flexible

## Notas

- El frontend quedó desacoplado y listo para evolucionar luego hacia despliegue en Vercel.
- Las imágenes se almacenan localmente en `wwwroot/uploads`.
- La base SQLite se crea automáticamente al iniciar el backend si no existe, en `App_Data/panini2026-tracker.db`.
- La licencia del proyecto es MIT, ver [LICENSE](./LICENSE).
