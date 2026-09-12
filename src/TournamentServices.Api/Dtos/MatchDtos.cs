namespace TournamentServices.Api.Dtos;

// PERSONA 4
public record ScoreDto(int HomeTeamScore, int VisitorTeamScore);
public record MatchDto(
    string Id, string TournamentId, string? GroupId,
    string HomeTeamId, string VisitorTeamId,
    TeamDto? HomeTeam, TeamDto? VisitorTeam,
    ScoreDto Score, string? Winner, bool IsCompleted); // Winner: "HOME" | "VISITOR" | null
public record CreateMatchDto(string? GroupId, string HomeTeamId, string VisitorTeamId);
public record UpdateScoreDto(int HomeTeamScore, int VisitorTeamScore);
