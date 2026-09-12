namespace TournamentServices.Api.Dtos;

// PERSONA 3
public record GroupDto(string Id, string Name, string TournamentId, List<TeamDto> Teams);
public record CreateGroupDto(string Name);
public record UpdateGroupDto(string Name);
public record AssignTeamsDto(List<string> TeamIds);
