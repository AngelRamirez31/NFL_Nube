using TournamentServices.Api.Common;
using TournamentServices.Api.Dtos;
using TournamentServices.Delegates;

namespace TournamentServices.Api.Routes;

// ============================================================
// PERSONA 2 — Tournaments
// Sigue el patrón de TeamRoutes.cs. Faltan los TODO marcados abajo:
// mapear domain <-> dto, y llamar a ITournamentDelegate una vez que
// dejes de lanzar NotImplementedException ahí.
// ============================================================
public static class TournamentRoutes
{
    public static void MapTournamentRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/tournaments").WithTags("Tournaments");

        group.MapGet("/", async (ITournamentDelegate tournamentDelegate) =>
        {
            // TODO: PERSONA 2 — listar todos y mapear a TournamentDto
            var tournaments = await tournamentDelegate.GetAllAsync();
            return Results.Ok(tournaments); // TODO: reemplazar por .Select(ToDto)
        });

        group.MapGet("/{tournamentId}", async (string tournamentId, ITournamentDelegate tournamentDelegate) =>
        {
            if (!Ids.IsValid(tournamentId)) return Results.BadRequest();

            // TODO: PERSONA 2
            var tournament = await tournamentDelegate.GetByIdAsync(tournamentId);
            return tournament is null ? Results.NotFound() : Results.Ok(tournament); // TODO: ToDto
        });

        group.MapPost("/", async (CreateTournamentDto dto, ITournamentDelegate tournamentDelegate) =>
        {
            // TODO: PERSONA 2 — mapear dto -> Tournament. Format.Type solo acepta "NFL";
            // si viene otro valor, responde 400 (valídalo aquí o con FluentValidation).
            // var created = await tournamentDelegate.CreateAsync(...);
            // return Results.Created($"/tournaments/{created.Id}", ToDto(created));
            throw new NotImplementedException();
        });

        group.MapPut("/{tournamentId}", async (string tournamentId, UpdateTournamentDto dto, ITournamentDelegate tournamentDelegate) =>
        {
            // TODO: PERSONA 2
            throw new NotImplementedException();
        });

        group.MapPatch("/{tournamentId}", async (string tournamentId, PatchTournamentDto dto, ITournamentDelegate tournamentDelegate) =>
        {
            // TODO: PERSONA 2 — solo actualizar los campos que vengan no-nulos
            var updated = await tournamentDelegate.PatchAsync(
                tournamentId, dto.Name, dto.Format?.NumberOfGroups, dto.Format?.MaxTeamsPerGroup, dto.Format?.Type);
            return updated is null ? Results.NotFound() : Results.Ok(updated); // TODO: ToDto
        });

        group.MapDelete("/{tournamentId}", async (string tournamentId, ITournamentDelegate tournamentDelegate) =>
        {
            var deleted = await tournamentDelegate.DeleteAsync(tournamentId);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }

    // TODO: PERSONA 2 — implementar el mapeo Tournament -> TournamentDto (incluye Format, Groups, Matches)
    // private static TournamentDto ToDto(Tournament tournament) => ...
}
