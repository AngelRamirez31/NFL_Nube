using TournamentServices.Api.Common;
using TournamentServices.Delegates;
using TournamentServices.Domain;

namespace TournamentServices.Api.Routes;

public static class TeamRoutes
{
    public static void MapTeamRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/teams").WithTags("Teams");

        group.MapGet("/", async (ITeamDelegate teamDelegate) =>
        {
            var result = await teamDelegate.GetAllAsync();
            return result.ToHttp();
        });

        group.MapGet("/{teamId}", async (string teamId, ITeamDelegate teamDelegate) =>
        {
            if (!Ids.IsValid(teamId)) return Results.BadRequest();

            var result = await teamDelegate.GetByIdAsync(teamId);
            return result.ToHttp();
        });

        group.MapPost("/", async (Team team, ITeamDelegate teamDelegate) =>
        {
            var result = await teamDelegate.CreateAsync(team);
            return result.ToCreatedHttp(created => $"/teams/{created.Id}");
        })
        .AddEndpointFilter<ValidationFilter<Team>>();

        group.MapPut("/{teamId}", async (string teamId, Team team, ITeamDelegate teamDelegate) =>
        {
            if (!Ids.IsValid(teamId)) return Results.BadRequest();

            var result = await teamDelegate.UpdateAsync(teamId, team);
            return result.ToHttp();
        })
        .AddEndpointFilter<ValidationFilter<Team>>();

        group.MapDelete("/{teamId}", async (string teamId, ITeamDelegate teamDelegate) =>
        {
            if (!Ids.IsValid(teamId)) return Results.BadRequest();

            var result = await teamDelegate.DeleteAsync(teamId);
            return result.ToNoContentHttp();
        });
    }
}
