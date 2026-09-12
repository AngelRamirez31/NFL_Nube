namespace TournamentServices.Api.Dtos;

// PERSONA 1
public record TeamDto(string Id, string Name);
public record CreateTeamDto(string Name);
public record UpdateTeamDto(string Name);
