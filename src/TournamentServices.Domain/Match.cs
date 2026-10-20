namespace TournamentServices.Domain;

public class Match
{
    public string Id { get; set; } = string.Empty;
    public string TournamentId { get; set; } = string.Empty;
    public string? GroupId { get; set; }
    public string HomeTeamId { get; set; } = string.Empty;
    public string VisitorTeamId { get; set; } = string.Empty;

    // Es null mientras el partido no tenga marcador.
    public Score? Score { get; set; }

    // Se llenan al leer para mostrar el nombre del equipo. No se guardan en la base.
    public Team? HomeTeam { get; set; }

    public Team? VisitorTeam { get; set; }

    // Se calculan a partir del Score.
    public bool IsCompleted => Score is not null;
    public Enums.Winner? Winner => Score?.GetWinner();
}
