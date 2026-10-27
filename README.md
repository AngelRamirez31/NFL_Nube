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

The tournament follows the NFL format. The rules live in `src/TournamentServices.Domain/TournamentRules.cs`.

Groups work like NFL divisions, and they are split into two conferences. Groups are sorted by name: the first half is the **AFC** and the second half is the **NFC** (with an odd number of groups, the AFC gets one more). Naming the groups `AFC East`, `AFC North`, ..., `NFC West` gives the real NFL layout. `GET /tournaments/{id}/standings` shows the conference of each group.

1. Create the tournament, the teams, and as many groups as `format.numberOfGroups` (at least 2, one per conference). Assign at least 2 teams to each group.
2. `POST /tournaments/{id}/matches/generate` creates the **group stage** (regular season): every team plays every other team in its group once.
3. Set each score with `PATCH /tournaments/{id}/matches/{matchId}/score`. Check the table with `GET /tournaments/{id}/standings`.
4. Call `generate` again to create the **Wild Card round**. Each conference sends 7 teams to the playoffs:
   - Seeds 1-4 are the division winners (first place of each group), ordered by record.
   - Seeds 5-7 are the **wild cards**: the best records among the other teams of the conference, from any group.
   - Seed 1 gets a **bye** (rests this round). The games are 2 vs 7, 3 vs 6, and 4 vs 5. The better seed plays at home.
5. After each round is played, call `generate` again. The teams are **reseeded** every round: the best remaining seed plays the worst remaining seed. Each conference plays the **Divisional round** and then the **Conference Championship**.
6. When the AFC and the NFC have a champion, `generate` creates the **Super Bowl** (`stage: SUPER_BOWL`). The team with the better regular-season record is listed as home.
7. `GET /tournaments/{id}/champion` returns the winner of the Super Bowl.

With 8 groups of 4 teams (32 teams) the tournament is the same as the real NFL: 14 playoff teams, then 6 Wild Card games, 4 Divisional games, 2 Conference Championships, and the Super Bowl. With smaller tournaments, a conference with fewer than 7 teams sends all of its teams, and the byes are adjusted so the bracket works.

Rules:

- New matches can't be generated while there are matches without a score (`422`).
- A score can't be changed once the next round has been generated (`422`).
- Standings are ordered by win percentage, then wins, then point difference, then points scored.
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
