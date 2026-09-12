using TournamentServices.Api.Dtos;
using TournamentServices.Api.Common;
using TournamentServices.Delegates;
using TournamentServices.Domain;

namespace TournamentServices.Api.Routes;

// ============================================================
// PERSONA 1 — Teams
// Esta es la ÚNICA ruta ya resuelta de punta a punta, para que sirva
// de ejemplo del patrón que deben seguir Tournament/Group/MatchRoutes.
// Ajusta lo que haga falta cuando tu TeamDelegate deje de lanzar
// NotImplementedException.
// ============================================================
public static class TeamRoutes
{
    public static void MapTeamRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/teams").WithTags("Teams");

        group.MapGet("/", async (ITeamDelegate teamDelegate) =>
        {
            var teams = await teamDelegate.GetAllAsync();
            return Results.Ok(teams.Select(ToDto));
        });

        group.MapGet("/{teamId}", async (string teamId, ITeamDelegate teamDelegate) =>
        {
            if (!Ids.IsValid(teamId)) return Results.BadRequest();

            var team = await teamDelegate.GetByIdAsync(teamId);
            return team is null ? Results.NotFound() : Results.Ok(ToDto(team));
        });

        group.MapPost("/", async (CreateTeamDto dto, ITeamDelegate teamDelegate) =>
        {
            var created = await teamDelegate.CreateAsync(new Team { Name = dto.Name });
            return Results.Created($"/teams/{created.Id}", ToDto(created));
        });

        group.MapPut("/{teamId}", async (string teamId, UpdateTeamDto dto, ITeamDelegate teamDelegate) =>
        {
            var updated = await teamDelegate.UpdateAsync(teamId, new Team { Name = dto.Name });
            return updated is null ? Results.NotFound() : Results.Ok(ToDto(updated));
        });

        group.MapDelete("/{teamId}", async (string teamId, ITeamDelegate teamDelegate) =>
        {
            var deleted = await teamDelegate.DeleteAsync(teamId);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }

    private static TeamDto ToDto(Team team) => new(team.Id, team.Name);
}
