using TournamentServices.Api.Common;
using TournamentServices.Api.Dtos;
using TournamentServices.Delegates;
using TournamentServices.Domain;

namespace TournamentServices.Api.Routes;

public static class MatchRoutes
{
    public static void MapMatchRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/tournaments/{tournamentId}/matches").WithTags("Matches");

        group.MapGet("/", async (string tournamentId, IMatchDelegate matchDelegate) =>
        {
            if (!Ids.IsValid(tournamentId)) return Results.BadRequest();

            var result = await matchDelegate.GetByTournamentAsync(tournamentId);
            return result.ToHttp(matches => matches.Select(ToDto));
        });

        group.MapGet("/{matchId}", async (string tournamentId, string matchId, IMatchDelegate matchDelegate) =>
        {
            if (!Ids.IsValid(tournamentId) || !Ids.IsValid(matchId)) return Results.BadRequest();

            var result = await matchDelegate.GetByIdAsync(tournamentId, matchId);
            return result.ToHttp(ToDto);
        });

        group.MapPost("/", async (string tournamentId, CreateMatchDto dto, IMatchDelegate matchDelegate) =>
        {
            if (!Ids.IsValid(tournamentId)) return Results.BadRequest();

            var result = await matchDelegate.CreateAsync(tournamentId, dto.GroupId, dto.HomeTeamId, dto.VisitorTeamId);
            return result.ToCreatedHttp(m => $"/tournaments/{tournamentId}/matches/{m.Id}", ToDto);
        })
        .AddEndpointFilter<ValidationFilter<CreateMatchDto>>();

        group.MapPatch("/{matchId}/score", async (string tournamentId, string matchId, UpdateScoreDto dto, IMatchDelegate matchDelegate) =>
        {
            if (!Ids.IsValid(tournamentId) || !Ids.IsValid(matchId)) return Results.BadRequest();

            var result = await matchDelegate.UpdateScoreAsync(tournamentId, matchId, dto.HomeTeamScore, dto.VisitorTeamScore);
            return result.ToHttp(ToDto);
        })
        .AddEndpointFilter<ValidationFilter<UpdateScoreDto>>();

        group.MapDelete("/{matchId}", async (string tournamentId, string matchId, IMatchDelegate matchDelegate) =>
        {
            if (!Ids.IsValid(tournamentId) || !Ids.IsValid(matchId)) return Results.BadRequest();

            var result = await matchDelegate.DeleteAsync(tournamentId, matchId);
            return result.ToNoContentHttp();
        });
    }

    private static MatchDto ToDto(Match match) => new(
        match.Id,
        match.TournamentId,
        match.GroupId,
        match.HomeTeamId,
        match.VisitorTeamId,
        match.HomeTeam is null ? null : new TeamDto(match.HomeTeam.Id, match.HomeTeam.Name),
        match.VisitorTeam is null ? null : new TeamDto(match.VisitorTeam.Id, match.VisitorTeam.Name),
        // El contrato siempre expone un objeto score; un partido sin jugar se
        // distingue por isCompleted = false, no por score ausente.
        new ScoreDto(match.Score?.HomeTeamScore ?? 0, match.Score?.VisitorTeamScore ?? 0),
        match.Winner?.ToString(),
        match.IsCompleted);
}
