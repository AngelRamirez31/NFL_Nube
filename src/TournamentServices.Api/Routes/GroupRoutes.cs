using TournamentServices.Api.Common;
using TournamentServices.Api.Dtos;
using TournamentServices.Delegates;

namespace TournamentServices.Api.Routes;

// ============================================================
// PERSONA 3 — Groups
// Sigue el patrón de TeamRoutes.cs (Result<T> + .ToHttp()/.ToCreatedHttp()/
// .ToNoContentHttp()).
// ============================================================
public static class GroupRoutes
{
    public static void MapGroupRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/tournaments/{tournamentId}/groups").WithTags("Groups");

        group.MapGet("/", async (string tournamentId, IGroupDelegate groupDelegate) =>
        {
            var result = await groupDelegate.GetByTournamentAsync(tournamentId);
            return result.ToHttp(groups => groups); // TODO: PERSONA 3 -> .Select(ToDto)
        });

        group.MapGet("/{groupId}", async (string tournamentId, string groupId, IGroupDelegate groupDelegate) =>
        {
            if (!Ids.IsValid(groupId)) return Results.BadRequest();

            var result = await groupDelegate.GetByIdAsync(tournamentId, groupId);
            return result.ToHttp(g => g); // TODO: PERSONA 3 -> ToDto
        });

        group.MapPost("/", async (string tournamentId, CreateGroupDto dto, IGroupDelegate groupDelegate) =>
        {
            var result = await groupDelegate.CreateAsync(tournamentId, dto.Name);
            return result.ToCreatedHttp(g => $"/tournaments/{tournamentId}/groups/{g.Id}", g => g); // TODO: ToDto
        })
        .AddEndpointFilter<ValidationFilter<CreateGroupDto>>();

        group.MapPut("/{groupId}", async (string tournamentId, string groupId, UpdateGroupDto dto, IGroupDelegate groupDelegate) =>
        {
            if (!Ids.IsValid(groupId)) return Results.BadRequest();

            var result = await groupDelegate.UpdateAsync(tournamentId, groupId, dto.Name);
            return result.ToHttp(g => g); // TODO: PERSONA 3 -> ToDto
        })
        .AddEndpointFilter<ValidationFilter<UpdateGroupDto>>();

        group.MapDelete("/{groupId}", async (string tournamentId, string groupId, IGroupDelegate groupDelegate) =>
        {
            if (!Ids.IsValid(groupId)) return Results.BadRequest();

            var result = await groupDelegate.DeleteAsync(tournamentId, groupId);
            return result.ToNoContentHttp();
        });

        group.MapPatch("/{groupId}/teams", async (string tournamentId, string groupId, AssignTeamsDto dto, IGroupDelegate groupDelegate) =>
        {
            if (!Ids.IsValid(groupId)) return Results.BadRequest();

            var result = await groupDelegate.AssignTeamsAsync(tournamentId, groupId, dto.TeamIds);
            return result.ToNoContentHttp();
        });
    }

    // TODO: PERSONA 3 — implementar el mapeo Group -> GroupDto (incluye Teams)
}
