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

## How a Tournament Is Played

The rules live in `src/TournamentServices.Domain/TournamentRules.cs`.

1. Create the tournament, the teams, and as many groups as `format.numberOfGroups`. Assign at least 2 teams to each group.
2. `POST /tournaments/{id}/matches/generate` creates the **group stage**: every team plays every other team in its group once.
3. Set each score with `PATCH /tournaments/{id}/matches/{matchId}/score`. Check the table with `GET /tournaments/{id}/standings`.
4. Call `generate` again to create the **playoffs**. The top 2 teams of each group qualify. Group winners are seeded first, then the runners-up. Seed 1 plays the last seed, seed 2 plays the second-to-last, and so on. The better seed plays at home. If the number of qualified teams is not 2, 4, 8..., the worst seeds are left out.
5. After each round is played, call `generate` again. The winners are reseeded and play the next round, until the final.
6. `GET /tournaments/{id}/champion` returns the winner of the final.

Rules:

- New matches can't be generated while there are matches without a score (`422`).
- A score can't be changed once the next round has been generated (`422`).
- Standings are ordered by wins, then point difference, then points scored.
- A tie counts as a home win.

## Tests

```powershell
dotnet test TournamentServices.sln
```

To run a load test, first start the API and then run:

```powershell
pip install locust
locust -f load_test/locustfile.py --host http://localhost:<puerto>
```
