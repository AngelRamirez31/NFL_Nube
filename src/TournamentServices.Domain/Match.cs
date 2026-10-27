namespace TournamentServices.Domain;

public class Match
{
    public string Id { get; set; } = string.Empty;
    public string TournamentId { get; set; } = string.Empty;
    public string? GroupId { get; set; }
    public string HomeTeamId { get; set; } = string.Empty;
    public string VisitorTeamId { get; set; } = string.Empty;

    // Fase del partido: fase de grupos o playoffs.
    public Enums.MatchStage Stage { get; set; } = Enums.MatchStage.GROUP;

    // Ronda de playoffs (1, 2, 3...). En fase de grupos siempre es 1.
    public int Round { get; set; } = 1;

    // Conferencia del partido de playoffs (AFC o NFC). Es null en grupos y en el Super Bowl.
    public string? Conference { get; set; }

    // Es null mientras el partido no tenga marcador.
    public Score? Score { get; set; }

    // Se llenan al leer para mostrar el nombre del equipo. No se guardan en la base.
    public Team? HomeTeam { get; set; }

    public Team? VisitorTeam { get; set; }

    // Se calculan a partir del Score.
    public bool IsCompleted => Score is not null;
    public Enums.Winner? Winner => Score?.GetWinner();

    // Id del equipo ganador (null si aún no se juega o si fue empate).
    public string? WinnerTeamId => Winner switch
    {
        Enums.Winner.HOME => HomeTeamId,
        Enums.Winner.VISITOR => VisitorTeamId,
        _ => null
    };

    // Id del equipo que perdió (null si aún no se juega o si fue empate).
    public string? LoserTeamId => Winner switch
    {
        Enums.Winner.HOME => VisitorTeamId,
        Enums.Winner.VISITOR => HomeTeamId,
        _ => null
    };
}
