using TournamentServices.Domain.Enums;

namespace TournamentServices.Domain;

// PERSONA 4 (Matches) es la dueña de esta clase.
public class Score
{
    public int HomeTeamScore { get; set; }
    public int VisitorTeamScore { get; set; }

    // Replica la regla del proyecto de referencia en C++ (domain/Match.hpp):
    // visitor gana solo si su marcador es estrictamente mayor; en cualquier otro caso gana home.
    // Revisen entre PERSONA 4 y el profesor si un empate debe tratarse distinto.
    public Winner GetWinner() =>
        VisitorTeamScore > HomeTeamScore ? Winner.VISITOR : Winner.HOME;
}
