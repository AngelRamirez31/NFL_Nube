# NFL Nube — Tournament Services

A REST API for managing American football tournaments, including teams, tournaments, groups, and matches. The project uses ASP.NET Core Minimal API, PostgreSQL, and a layered architecture.

## Components

- `src/TournamentServices.Api`: HTTP endpoints, validation, and API documentation.
- `src/TournamentServices.Domain`: entities and business rules.
- `src/TournamentServices.Delegates`: application use-case coordination.
- `src/TournamentServices.Repositories`: PostgreSQL data access.
- `database/`: database initialization scripts.
- `tests/`: domain, repository, delegate, and API tests.
- `load_test/`: Locust load test.

## Requirements

- .NET SDK 10.
- Docker or Podman to run PostgreSQL.

## Run Locally

1. Start the database from the project root:

   ```powershell
   docker compose up -d
   ```

2. Restore dependencies and run the API:

   ```powershell
   dotnet restore TournamentServices.sln
   dotnet run --project src/TournamentServices.Api
   ```

3. Confirm that the service responds at `/health`. Interactive API documentation is available at `/scalar/v1` on the port reported by `dotnet run`.

The development configuration uses PostgreSQL at `localhost:5432`, database `tournament_db`, and user `tournament_svc`. The scripts in `database/` run when the container is created.

## Tests

```powershell
dotnet test TournamentServices.sln
```

To run a load test, first start the API and then run:

```powershell
pip install locust
locust -f load_test/locustfile.py --host http://localhost:<puerto>
```
