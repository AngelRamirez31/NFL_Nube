using TournamentServices.Api.Common;
using TournamentServices.Api.Dtos;
using TournamentServices.Delegates;

namespace TournamentServices.Api.Routes;

// ============================================================
// PERSONA 4 — Matches
// Sigue el patrón de TeamRoutes.cs (Result<T> + .ToHttp()/.ToCreatedHttp()/
// .ToNoContentHttp()).
// ============================================================
public static class MatchRoutes
{
    public static void MapMatchRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/tournaments/{tournamentId}/matches").WithTags("Matches");

        group.MapGet("/", async (string tournamentId, IMatchDelegate matchDelegate) =>
        {
            var result = await matchDelegate.GetByTournamentAsync(tournamentId);
            return result.ToHttp(matches => matches); // TODO: PERSONA 4 -> .Select(ToDto)
        });

        group.MapGet("/{matchId}", async (string tournamentId, string matchId, IMatchDelegate matchDelegate) =>
        {
            if (!Ids.IsValid(matchId)) return Results.BadRequest();

            var result = await matchDelegate.GetByIdAsync(tournamentId, matchId);
            return result.ToHttp(m => m); // TODO: PERSONA 4 -> ToDto
        });

        group.MapPost("/", async (string tournamentId, CreateMatchDto dto, IMatchDelegate matchDelegate) =>
        {
            var result = await matchDelegate.CreateAsync(tournamentId, dto.GroupId, dto.HomeTeamId, dto.VisitorTeamId);
            return result.ToCreatedHttp(m => $"/tournaments/{tournamentId}/matches/{m.Id}", m => m); // TODO: ToDto
        })
        .AddEndpointFilter<ValidationFilter<CreateMatchDto>>();

        group.MapPatch("/{matchId}/score", async (string tournamentId, string matchId, UpdateScoreDto dto, IMatchDelegate matchDelegate) =>
        {
            if (!Ids.IsValid(matchId)) return Results.BadRequest();

            var result = await matchDelegate.UpdateScoreAsync(tournamentId, matchId, dto.HomeTeamScore, dto.VisitorTeamScore);
            return result.ToHttp(m => m); // TODO: PERSONA 4 -> ToDto
        })
        .AddEndpointFilter<ValidationFilter<UpdateScoreDto>>();

        group.MapDelete("/{matchId}", async (string tournamentId, string matchId, IMatchDelegate matchDelegate) =>
        {
            if (!Ids.IsValid(matchId)) return Results.BadRequest();

            var result = await matchDelegate.DeleteAsync(tournamentId, matchId);
            return result.ToNoContentHttp();
        });
    }

    // TODO: PERSONA 4 — implementar el mapeo Match -> MatchDto
    // (incluye Score, Winner y IsCompleted, que ya vienen calculados en Domain/Match.cs)
}
