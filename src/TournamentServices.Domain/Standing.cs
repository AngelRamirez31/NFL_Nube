namespace TournamentServices.Domain;

// Fila de la tabla de posiciones: el récord de un equipo en su grupo.
public class Standing
{
    public Team Team { get; set; } = new();
    public int Played { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int PointsFor { get; set; }
    public int PointsAgainst { get; set; }

    // Puntos anotados menos puntos recibidos.
    public int PointDifference => PointsFor - PointsAgainst;
}
