namespace TournamentServices.Domain;

// PERSONA 4 (Matches) es la dueña de esta clase.
public class Match
{
    public string Id { get; set; } = string.Empty;
    public string TournamentId { get; set; } = string.Empty;
    public string? GroupId { get; set; }
    public string HomeTeamId { get; set; } = string.Empty;
    public string VisitorTeamId { get; set; } = string.Empty;
    public Score Score { get; set; } = new();
    public bool IsCompleted { get; set; }
}
