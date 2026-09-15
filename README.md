# NFL Nube — Tournament Services

API REST para administrar torneos de fútbol americano: equipos, torneos, grupos y partidos. El proyecto está construido con ASP.NET Core Minimal API, PostgreSQL y una arquitectura en capas.

## Componentes

- `src/TournamentServices.Api`: endpoints HTTP, validaciones y documentación de la API.
- `src/TournamentServices.Domain`: entidades y reglas de negocio.
- `src/TournamentServices.Delegates`: coordinación de casos de uso.
- `src/TournamentServices.Repositories`: acceso a PostgreSQL.
- `database/`: scripts de inicialización de la base de datos.
- `tests/`: pruebas de dominio, repositorios, delegados y API.
- `load_test/`: prueba de carga con Locust.

## Requisitos

- .NET SDK 10.
- Docker o Podman para ejecutar PostgreSQL.

## Ejecutar localmente

1. Inicia la base de datos desde la raíz del proyecto:

   ```powershell
   docker compose up -d
   ```

2. Restaura dependencias y ejecuta la API:

   ```powershell
   dotnet restore TournamentServices.sln
   dotnet run --project src/TournamentServices.Api
   ```

3. Comprueba que el servicio responde en `/health`. La documentación interactiva queda disponible en `/scalar/v1` (en el puerto que indique `dotnet run`).

La configuración de desarrollo usa PostgreSQL en `localhost:5432`, base `tournament_db` y el usuario `tournament_svc`. Los scripts de `database/` se cargan al crear el contenedor.

## Pruebas

```powershell
dotnet test TournamentServices.sln
```

Para una prueba de carga, con la API activa:

```powershell
pip install locust
locust -f load_test/locustfile.py --host http://localhost:<puerto>
```

