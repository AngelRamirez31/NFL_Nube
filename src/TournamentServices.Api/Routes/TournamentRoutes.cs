using TournamentServices.Api.Common;
using TournamentServices.Api.Dtos;
using TournamentServices.Delegates;
using TournamentServices.Domain;
using TournamentServices.Domain.Enums;

namespace TournamentServices.Api.Routes;

public static class TournamentRoutes
{
    public static void MapTournamentRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/tournaments").WithTags("Tournaments");

        group.MapGet("/", async (ITournamentDelegate tournamentDelegate) =>
        {
            var result = await tournamentDelegate.GetAllAsync();
            return result.ToHttp(tournaments => tournaments.Select(ToDto));
        });

        group.MapGet("/{tournamentId}", async (string tournamentId, ITournamentDelegate tournamentDelegate) =>
        {
            if (!Ids.IsValid(tournamentId)) return Results.BadRequest();

            var result = await tournamentDelegate.GetByIdAsync(tournamentId);
            return result.ToHttp(ToDto);
        });

        group.MapPost("/", async (CreateTournamentDto dto, ITournamentDelegate tournamentDelegate) =>
        {
            var result = await tournamentDelegate.CreateAsync(ToDomain(dto.Name, dto.Format));
            return result.ToCreatedHttp(t => $"/tournaments/{t.Id}", ToDto);
        })
        .AddEndpointFilter<ValidationFilter<CreateTournamentDto>>();

        group.MapPut("/{tournamentId}", async (string tournamentId, UpdateTournamentDto dto, ITournamentDelegate tournamentDelegate) =>
        {
            if (!Ids.IsValid(tournamentId)) return Results.BadRequest();

            var result = await tournamentDelegate.UpdateAsync(tournamentId, ToDomain(dto.Name, dto.Format));
            return result.ToHttp(ToDto);
        })
        .AddEndpointFilter<ValidationFilter<UpdateTournamentDto>>();

        group.MapPatch("/{tournamentId}", async (string tournamentId, PatchTournamentDto dto, ITournamentDelegate tournamentDelegate) =>
        {
            if (!Ids.IsValid(tournamentId)) return Results.BadRequest();

            var result = await tournamentDelegate.PatchAsync(
                tournamentId, dto.Name, dto.Format?.NumberOfGroups, dto.Format?.MaxTeamsPerGroup, dto.Format?.Type);
            return result.ToHttp(ToDto);
        });

        group.MapDelete("/{tournamentId}", async (string tournamentId, ITournamentDelegate tournamentDelegate) =>
        {
            if (!Ids.IsValid(tournamentId)) return Results.BadRequest();

            var result = await tournamentDelegate.DeleteAsync(tournamentId);
            return result.ToNoContentHttp();
        });
    }

    // El validador ya garantizó que Type sea "NFL" antes de llegar aquí.
    private static Tournament ToDomain(string name, TournamentFormatDto format) => new()
    {
        Name = name,
        Format = new TournamentFormat
        {
            NumberOfGroups = format.NumberOfGroups,
            MaxTeamsPerGroup = format.MaxTeamsPerGroup,
            Type = TournamentType.NFL
        }
    };

    private static TournamentDto ToDto(Tournament tournament) => new(
        tournament.Id,
        tournament.Name,
        new TournamentFormatDto(
            tournament.Format.MaxTeamsPerGroup,
            tournament.Format.NumberOfGroups,
            tournament.Format.Type.ToString()),
        tournament.Groups.Select(GroupRoutes.ToDto).ToList(),
        tournament.Matches.Select(MatchRoutes.ToDto).ToList());
}
