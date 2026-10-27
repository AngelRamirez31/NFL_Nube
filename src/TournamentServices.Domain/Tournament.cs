namespace TournamentServices.Domain;

// Torneo con su formato, sus grupos y sus partidos.
public class Tournament
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TournamentFormat Format { get; set; } = new();

    // Se llenan al leer el torneo. No se guardan dentro del JSONB del torneo.
    public List<Group> Groups { get; set; } = new();

    public List<Match> Matches { get; set; } = new();
}
