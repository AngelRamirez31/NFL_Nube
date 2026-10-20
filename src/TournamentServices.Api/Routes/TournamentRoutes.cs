using System.Text.Json.Nodes;
using TournamentServices.Api.Common;
using TournamentServices.Delegates;
using TournamentServices.Domain;

namespace TournamentServices.Api.Routes;

public static class TournamentRoutes
{
    public static void MapTournamentRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/tournaments").WithTags("Tournaments");

        group.MapGet("/", async (ITournamentDelegate tournamentDelegate) =>
        {
            var result = await tournamentDelegate.GetAllAsync();
            return result.ToHttp();
        });

        group.MapGet("/{tournamentId}", async (string tournamentId, ITournamentDelegate tournamentDelegate) =>
        {
            if (!Ids.IsValid(tournamentId)) return Results.BadRequest();

            var result = await tournamentDelegate.GetByIdAsync(tournamentId);
            return result.ToHttp();
        });

        group.MapPost("/", async (Tournament tournament, ITournamentDelegate tournamentDelegate) =>
        {
            var result = await tournamentDelegate.CreateAsync(tournament);
            return result.ToCreatedHttp(t => $"/tournaments/{t.Id}");
        })
        .AddEndpointFilter<ValidationFilter<Tournament>>();

        group.MapPut("/{tournamentId}", async (string tournamentId, Tournament tournament, ITournamentDelegate tournamentDelegate) =>
        {
            if (!Ids.IsValid(tournamentId)) return Results.BadRequest();

            var result = await tournamentDelegate.UpdateAsync(tournamentId, tournament);
            return result.ToHttp();
        })
        .AddEndpointFilter<ValidationFilter<Tournament>>();

        // Se lee el JSON directamente para saber qué campos se enviaron.
        group.MapPatch("/{tournamentId}", async (string tournamentId, JsonObject body, ITournamentDelegate tournamentDelegate) =>
        {
            if (!Ids.IsValid(tournamentId)) return Results.BadRequest();

            var format = body["format"];
            var result = await tournamentDelegate.PatchAsync(
                tournamentId,
                (string?)body["name"],
                (int?)format?["numberOfGroups"],
                (int?)format?["maxTeamsPerGroup"],
                (string?)format?["type"]);
            return result.ToHttp();
        });

        group.MapDelete("/{tournamentId}", async (string tournamentId, ITournamentDelegate tournamentDelegate) =>
        {
            if (!Ids.IsValid(tournamentId)) return Results.BadRequest();

            var result = await tournamentDelegate.DeleteAsync(tournamentId);
            return result.ToNoContentHttp();
        });
    }
}
