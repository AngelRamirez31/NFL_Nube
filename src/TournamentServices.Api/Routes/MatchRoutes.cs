using TournamentServices.Api.Common;
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
            return result.ToHttp();
        });

        group.MapGet("/{matchId}", async (string tournamentId, string matchId, IMatchDelegate matchDelegate) =>
        {
            if (!Ids.IsValid(tournamentId) || !Ids.IsValid(matchId)) return Results.BadRequest();

            var result = await matchDelegate.GetByIdAsync(tournamentId, matchId);
            return result.ToHttp();
        });

        group.MapPost("/", async (string tournamentId, Match match, IMatchDelegate matchDelegate) =>
        {
            if (!Ids.IsValid(tournamentId)) return Results.BadRequest();

            var result = await matchDelegate.CreateAsync(tournamentId, match.GroupId, match.HomeTeamId, match.VisitorTeamId);
            return result.ToCreatedHttp(m => $"/tournaments/{tournamentId}/matches/{m.Id}");
        })
        .AddEndpointFilter<ValidationFilter<Match>>();

        group.MapPatch("/{matchId}/score", async (string tournamentId, string matchId, Score score, IMatchDelegate matchDelegate) =>
        {
            if (!Ids.IsValid(tournamentId) || !Ids.IsValid(matchId)) return Results.BadRequest();

            var result = await matchDelegate.UpdateScoreAsync(tournamentId, matchId, score.HomeTeamScore, score.VisitorTeamScore);
            return result.ToHttp();
        })
        .AddEndpointFilter<ValidationFilter<Score>>();

        group.MapDelete("/{matchId}", async (string tournamentId, string matchId, IMatchDelegate matchDelegate) =>
        {
            if (!Ids.IsValid(tournamentId) || !Ids.IsValid(matchId)) return Results.BadRequest();

            var result = await matchDelegate.DeleteAsync(tournamentId, matchId);
            return result.ToNoContentHttp();
        });
    }
}
