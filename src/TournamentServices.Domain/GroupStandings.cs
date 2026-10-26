namespace TournamentServices.Domain;

// Tabla de posiciones de un grupo, del primer al último lugar.
public class GroupStandings
{
    public string GroupId { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public List<Standing> Standings { get; set; } = new();
}
