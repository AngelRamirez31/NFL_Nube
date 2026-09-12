using TournamentServices.Api.Common;
using TournamentServices.Api.Dtos;
using TournamentServices.Delegates;

namespace TournamentServices.Api.Routes;

// ============================================================
// PERSONA 4 — Matches
// Sigue el patrón de TeamRoutes.cs.
// ============================================================
public static class MatchRoutes
{
    public static void MapMatchRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/tournaments/{tournamentId}/matches").WithTags("Matches");

        group.MapGet("/", async (string tournamentId, IMatchDelegate matchDelegate) =>
        {
            // TODO: PERSONA 4
            var matches = await matchDelegate.GetByTournamentAsync(tournamentId);
            return Results.Ok(matches); // TODO: .Select(ToDto)
        });

        group.MapGet("/{matchId}", async (string tournamentId, string matchId, IMatchDelegate matchDelegate) =>
        {
            if (!Ids.IsValid(matchId)) return Results.BadRequest();

            // TODO: PERSONA 4
            var match = await matchDelegate.GetByIdAsync(tournamentId, matchId);
            return match is null ? Results.NotFound() : Results.Ok(match); // TODO: ToDto
        });

        group.MapPost("/", async (string tournamentId, CreateMatchDto dto, IMatchDelegate matchDelegate) =>
        {
            // TODO: PERSONA 4 — 422 si algún equipo no existe/pertenece al torneo, o si son el mismo equipo
            var created = await matchDelegate.CreateAsync(tournamentId, dto.GroupId, dto.HomeTeamId, dto.VisitorTeamId);
            if (created is null) return Results.UnprocessableEntity();
            return Results.Created($"/tournaments/{tournamentId}/matches/{created.Id}", created); // TODO: ToDto
        });

        group.MapPatch("/{matchId}/score", async (string tournamentId, string matchId, UpdateScoreDto dto, IMatchDelegate matchDelegate) =>
        {
            // TODO: PERSONA 4 — 400 si algún score es negativo (valídalo aquí o con FluentValidation)
            if (dto.HomeTeamScore < 0 || dto.VisitorTeamScore < 0) return Results.BadRequest();

            var updated = await matchDelegate.UpdateScoreAsync(tournamentId, matchId, dto.HomeTeamScore, dto.VisitorTeamScore);
            return updated is null ? Results.NotFound() : Results.Ok(updated); // TODO: ToDto
        });

        group.MapDelete("/{matchId}", async (string tournamentId, string matchId, IMatchDelegate matchDelegate) =>
        {
            var deleted = await matchDelegate.DeleteAsync(tournamentId, matchId);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }

    // TODO: PERSONA 4 — implementar el mapeo Match -> MatchDto (incluye Score, Winner calculado, IsCompleted)
}
