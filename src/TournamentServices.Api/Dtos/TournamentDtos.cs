namespace TournamentServices.Api.Dtos;

// PERSONA 2
public record TournamentFormatDto(int MaxTeamsPerGroup, int NumberOfGroups, string Type); // solo "NFL" — validar en el validator/delegate y rechazar cualquier otro valor
public record TournamentDto(string Id, string Name, TournamentFormatDto Format, List<GroupDto> Groups, List<MatchDto> Matches);
public record CreateTournamentDto(string Name, TournamentFormatDto Format);
public record UpdateTournamentDto(string Name, TournamentFormatDto Format);

public record PatchTournamentFormatDto(int? MaxTeamsPerGroup, int? NumberOfGroups, string? Type);
public record PatchTournamentDto(string? Name, PatchTournamentFormatDto? Format);
