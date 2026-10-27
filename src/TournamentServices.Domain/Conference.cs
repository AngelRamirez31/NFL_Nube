namespace TournamentServices.Domain;

// Conferencia de la NFL (AFC o NFC) con sus grupos (divisiones).
public class Conference
{
    public string Name { get; set; } = string.Empty;
    public List<Group> Groups { get; set; } = new();
}
