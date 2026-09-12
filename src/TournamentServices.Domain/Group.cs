namespace TournamentServices.Domain;

// PERSONA 3 (Groups) es la dueña de esta clase.
public class Group
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TournamentId { get; set; } = string.Empty;
    public List<Team> Teams { get; set; } = new();
}
