using TournamentServices.Api.Common;
using TournamentServices.Api.Dtos;
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

            return result.ToHttp(groups =>
                groups.Select(ToDto));
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

            return result.ToHttp(ToDto);
        });

        group.MapPost("/", async (
            string tournamentId,
            CreateGroupDto dto,
            IGroupDelegate groupDelegate) =>
        {
            if (!Ids.IsValid(tournamentId))
                return Results.BadRequest();

            var result =
                await groupDelegate.CreateAsync(
                    tournamentId,
                    dto.Name);

            return result.ToCreatedHttp(
                created =>
                    $"/tournaments/{tournamentId}/groups/{created.Id}",
                ToDto);
        })
        .AddEndpointFilter<ValidationFilter<CreateGroupDto>>();

        group.MapPut("/{groupId}", async (
            string tournamentId,
            string groupId,
            UpdateGroupDto dto,
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
                    dto.Name);

            return result.ToHttp(ToDto);
        })
        .AddEndpointFilter<ValidationFilter<UpdateGroupDto>>();

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
            AssignTeamsDto dto,
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
                    dto.TeamIds);

            return result.ToNoContentHttp();
        })
        .AddEndpointFilter<ValidationFilter<AssignTeamsDto>>();
    }

    private static GroupDto ToDto(Group group)
    {
        return new GroupDto(
            group.Id,
            group.Name,
            group.TournamentId,
            group.Teams
                .Select(team =>
                    new TeamDto(
                        team.Id,
                        team.Name))
                .ToList());
    }
}