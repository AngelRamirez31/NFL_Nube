using TournamentServices.Domain.Enums;

namespace TournamentServices.Domain;

public class Score
{
    public int HomeTeamScore { get; set; }
    public int VisitorTeamScore { get; set; }

    // Si quedan iguales es empate (como en la temporada regular de la NFL).
    public Winner GetWinner()
    {
        if (HomeTeamScore > VisitorTeamScore) return Winner.HOME;
        if (VisitorTeamScore > HomeTeamScore) return Winner.VISITOR;
        return Winner.TIE;
    }

}
