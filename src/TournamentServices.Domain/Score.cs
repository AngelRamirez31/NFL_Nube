using TournamentServices.Domain.Enums;

namespace TournamentServices.Domain;

// Marcador de un partido.
public class Score
{
    public int HomeTeamScore { get; set; }
    public int VisitorTeamScore { get; set; }

    // Si quedan iguales es empate (como en la temporada regular de la NFL).
    // Regresa quién ganó según los puntos.
    public Winner GetWinner()
    {
        if (HomeTeamScore > VisitorTeamScore) return Winner.HOME;
        if (VisitorTeamScore > HomeTeamScore) return Winner.VISITOR;
        return Winner.TIE;
    }

}
