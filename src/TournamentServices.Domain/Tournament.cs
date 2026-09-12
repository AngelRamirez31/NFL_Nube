namespace TournamentServices.Domain;

// PERSONA 2 (Tournaments) es la dueña de esta clase.
public class Tournament
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TournamentFormat Format { get; set; } = new();
    public List<Group> Groups { get; set; } = new();
    public List<Match> Matches { get; set; } = new();
}
