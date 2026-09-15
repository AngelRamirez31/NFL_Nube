using TournamentServices.Domain.Enums;

namespace TournamentServices.Domain;

public class Score
{
    public int HomeTeamScore { get; set; }
    public int VisitorTeamScore { get; set; }

    public Winner GetWinner() =>
        VisitorTeamScore > HomeTeamScore ? Winner.VISITOR : Winner.HOME;

}
