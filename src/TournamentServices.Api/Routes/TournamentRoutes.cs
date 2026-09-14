using TournamentServices.Api.Common;
using TournamentServices.Api.Dtos;
using TournamentServices.Delegates;

namespace TournamentServices.Api.Routes;

// ============================================================
// PERSONA 2 — Tournaments
// Sigue el patrón de TeamRoutes.cs (Result<T> + .ToHttp()/.ToCreatedHttp()/
// .ToNoContentHttp()). Faltan los TODO marcados abajo: mapear domain <-> dto
// y crear tus validadores en Api/Validators/TournamentValidators.cs.
// ============================================================
public static class TournamentRoutes
{
    public static void MapTournamentRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/tournaments").WithTags("Tournaments");

        group.MapGet("/", async (ITournamentDelegate tournamentDelegate) =>
        {
            var result = await tournamentDelegate.GetAllAsync();
            return result.ToHttp(tournaments => tournaments); // TODO: PERSONA 2 -> .Select(ToDto)
        });

        group.MapGet("/{tournamentId}", async (string tournamentId, ITournamentDelegate tournamentDelegate) =>
        {
            if (!Ids.IsValid(tournamentId)) return Results.BadRequest();

            var result = await tournamentDelegate.GetByIdAsync(tournamentId);
            return result.ToHttp(t => t); // TODO: PERSONA 2 -> ToDto
        });

        group.MapPost("/", async (CreateTournamentDto dto, ITournamentDelegate tournamentDelegate) =>
        {
            // TODO: PERSONA 2 — mapear dto -> Tournament (Format.Type solo acepta "NFL")
            // var result = await tournamentDelegate.CreateAsync(...);
            // return result.ToCreatedHttp(t => $"/tournaments/{t.Id}", ToDto);
            throw new NotImplementedException();
        })
        .AddEndpointFilter<ValidationFilter<CreateTournamentDto>>();

        group.MapPut("/{tournamentId}", async (string tournamentId, UpdateTournamentDto dto, ITournamentDelegate tournamentDelegate) =>
        {
            // TODO: PERSONA 2
            throw new NotImplementedException();
        })
        .AddEndpointFilter<ValidationFilter<UpdateTournamentDto>>();

        group.MapPatch("/{tournamentId}", async (string tournamentId, PatchTournamentDto dto, ITournamentDelegate tournamentDelegate) =>
        {
            if (!Ids.IsValid(tournamentId)) return Results.BadRequest();

            var result = await tournamentDelegate.PatchAsync(
                tournamentId, dto.Name, dto.Format?.NumberOfGroups, dto.Format?.MaxTeamsPerGroup, dto.Format?.Type);
            return result.ToHttp(t => t); // TODO: PERSONA 2 -> ToDto
        });

        group.MapDelete("/{tournamentId}", async (string tournamentId, ITournamentDelegate tournamentDelegate) =>
        {
            if (!Ids.IsValid(tournamentId)) return Results.BadRequest();

            var result = await tournamentDelegate.DeleteAsync(tournamentId);
            return result.ToNoContentHttp();
        });
    }

    // TODO: PERSONA 2 — implementar el mapeo Tournament -> TournamentDto
    // (incluye Format, y Groups/Matches consultando IGroupRepository/IMatchRepository)
}
