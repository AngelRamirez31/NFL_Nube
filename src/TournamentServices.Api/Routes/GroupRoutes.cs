using TournamentServices.Api.Common;
using TournamentServices.Delegates;
using TournamentServices.Domain;

namespace TournamentServices.Api.Routes;

public static class GroupRoutes
{
    public static void MapGroupRoutes(this WebApplication app)
    {
        var group = app
            .MapGroup("/tournaments/{tournamentId}/groups")
            .WithTags("Groups");

        group.MapGet("/", async (
            string tournamentId,
            IGroupDelegate groupDelegate) =>
        {
            if (!Ids.IsValid(tournamentId))
                return Results.BadRequest();

            var result =
                await groupDelegate.GetByTournamentAsync(tournamentId);

            return result.ToHttp();
        });

        group.MapGet("/{groupId}", async (
            string tournamentId,
            string groupId,
            IGroupDelegate groupDelegate) =>
        {
            if (!Ids.IsValid(tournamentId) ||
                !Ids.IsValid(groupId))
            {
                return Results.BadRequest();
            }

            var result =
                await groupDelegate.GetByIdAsync(
                    tournamentId,
                    groupId);

            return result.ToHttp();
        });

        group.MapPost("/", async (
            string tournamentId,
            Group body,
            IGroupDelegate groupDelegate) =>
        {
            if (!Ids.IsValid(tournamentId))
                return Results.BadRequest();

            var result =
                await groupDelegate.CreateAsync(
                    tournamentId,
                    body.Name);

            return result.ToCreatedHttp(
                created =>
                    $"/tournaments/{tournamentId}/groups/{created.Id}");
        })
        .AddEndpointFilter<ValidationFilter<Group>>();

        group.MapPut("/{groupId}", async (
            string tournamentId,
            string groupId,
            Group body,
            IGroupDelegate groupDelegate) =>
        {
            if (!Ids.IsValid(tournamentId) ||
                !Ids.IsValid(groupId))
            {
                return Results.BadRequest();
            }

            var result =
                await groupDelegate.UpdateAsync(
                    tournamentId,
                    groupId,
                    body.Name);

            return result.ToHttp();
        })
        .AddEndpointFilter<ValidationFilter<Group>>();

        group.MapDelete("/{groupId}", async (
            string tournamentId,
            string groupId,
            IGroupDelegate groupDelegate) =>
        {
            if (!Ids.IsValid(tournamentId) ||
                !Ids.IsValid(groupId))
            {
                return Results.BadRequest();
            }

            var result =
                await groupDelegate.DeleteAsync(
                    tournamentId,
                    groupId);

            return result.ToNoContentHttp();
        });

        group.MapPatch("/{groupId}/teams", async (
            string tournamentId,
            string groupId,
            List<Team> teams,
            IGroupDelegate groupDelegate) =>
        {
            if (!Ids.IsValid(tournamentId) ||
                !Ids.IsValid(groupId))
            {
                return Results.BadRequest();
            }

            var result =
                await groupDelegate.AssignTeamsAsync(
                    tournamentId,
                    groupId,
                    teams.Select(team => team.Id));

            return result.ToNoContentHttp();
        })
        .AddEndpointFilter<ValidationFilter<List<Team>>>();
    }
}