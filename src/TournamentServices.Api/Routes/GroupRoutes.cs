using TournamentServices.Api.Common;
using TournamentServices.Api.Dtos;
using TournamentServices.Delegates;

namespace TournamentServices.Api.Routes;

// ============================================================
// PERSONA 3 — Groups
// Sigue el patrón de TeamRoutes.cs.
// ============================================================
public static class GroupRoutes
{
    public static void MapGroupRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/tournaments/{tournamentId}/groups").WithTags("Groups");

        group.MapGet("/", async (string tournamentId, IGroupDelegate groupDelegate) =>
        {
            // TODO: PERSONA 3
            var groups = await groupDelegate.GetByTournamentAsync(tournamentId);
            return Results.Ok(groups); // TODO: .Select(ToDto)
        });

        group.MapGet("/{groupId}", async (string tournamentId, string groupId, IGroupDelegate groupDelegate) =>
        {
            if (!Ids.IsValid(groupId)) return Results.BadRequest();

            // TODO: PERSONA 3
            var found = await groupDelegate.GetByIdAsync(tournamentId, groupId);
            return found is null ? Results.NotFound() : Results.Ok(found); // TODO: ToDto
        });

        group.MapPost("/", async (string tournamentId, CreateGroupDto dto, IGroupDelegate groupDelegate) =>
        {
            // TODO: PERSONA 3 — devolver 404 si el torneo no existe, 422 si el nombre ya existe en el torneo
            var created = await groupDelegate.CreateAsync(tournamentId, dto.Name);
            if (created is null) return Results.UnprocessableEntity();
            return Results.Created($"/tournaments/{tournamentId}/groups/{created.Id}", created); // TODO: ToDto
        });

        group.MapPut("/{groupId}", async (string tournamentId, string groupId, UpdateGroupDto dto, IGroupDelegate groupDelegate) =>
        {
            // TODO: PERSONA 3
            throw new NotImplementedException();
        });

        group.MapDelete("/{groupId}", async (string tournamentId, string groupId, IGroupDelegate groupDelegate) =>
        {
            var deleted = await groupDelegate.DeleteAsync(tournamentId, groupId);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        group.MapPatch("/{groupId}/teams", async (string tournamentId, string groupId, AssignTeamsDto dto, IGroupDelegate groupDelegate) =>
        {
            // TODO: PERSONA 3 — el delegate devuelve: null = 404, false = 422, true = 204
            var result = await groupDelegate.AssignTeamsAsync(tournamentId, groupId, dto.TeamIds);
            return result switch
            {
                null => Results.NotFound(),
                false => Results.UnprocessableEntity(),
                true => Results.NoContent(),
            };
        });
    }

    // TODO: PERSONA 3 — implementar el mapeo Group -> GroupDto (incluye Teams)
}
