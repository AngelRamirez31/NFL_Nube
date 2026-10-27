namespace TournamentServices.Domain;

// Grupo (división) de un torneo con sus equipos.
public class Group
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TournamentId { get; set; } = string.Empty;
    public List<Team> Teams { get; set; } = new();
}
