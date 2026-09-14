using TournamentServices.Api.Common;
using TournamentServices.Api.Dtos;
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
            return result.ToHttp(teams => teams.Select(ToDto));
        });

        group.MapGet("/{teamId}", async (string teamId, ITeamDelegate teamDelegate) =>
        {
            if (!Ids.IsValid(teamId)) return Results.BadRequest();

            var result = await teamDelegate.GetByIdAsync(teamId);
            return result.ToHttp(ToDto);
        });

        group.MapPost("/", async (CreateTeamDto dto, ITeamDelegate teamDelegate) =>
        {
            var result = await teamDelegate.CreateAsync(new Team { Name = dto.Name });
            return result.ToCreatedHttp(team => $"/teams/{team.Id}", ToDto);
        })
        .AddEndpointFilter<ValidationFilter<CreateTeamDto>>();

        group.MapPut("/{teamId}", async (string teamId, UpdateTeamDto dto, ITeamDelegate teamDelegate) =>
        {
            if (!Ids.IsValid(teamId)) return Results.BadRequest();

            var result = await teamDelegate.UpdateAsync(teamId, new Team { Name = dto.Name });
            return result.ToHttp(ToDto);
        })
        .AddEndpointFilter<ValidationFilter<UpdateTeamDto>>();

        group.MapDelete("/{teamId}", async (string teamId, ITeamDelegate teamDelegate) =>
        {
            if (!Ids.IsValid(teamId)) return Results.BadRequest();

            var result = await teamDelegate.DeleteAsync(teamId);
            return result.ToNoContentHttp();
        });
    }

    private static TeamDto ToDto(Team team) => new(team.Id, team.Name);
}
