using TournamentServices.Domain.Enums;

namespace TournamentServices.Domain;

// PERSONA 4 (Matches) es la dueña de esta clase.
public class Score
{
    public int HomeTeamScore { get; set; }
    public int VisitorTeamScore { get; set; }

    // PENDIENTE DE CONFIRMAR CON EL PROFESOR: el proyecto de referencia en C++
    // (domain/Match.hpp) hardcodea el empate como HOME. El contrato (MatchDto)
    // declara winner: "HOME | VISITOR | null", así que null SÍ es representable
    // y en NFL el empate existe tras overtime. Mientras se confirma, se deja
    // el comportamiento del C++ (empate = HOME). Si el profesor confirma que
    // el empate debe ser null, cambiar a la versión comentada abajo y borrar
    // esta nota.
    public Winner GetWinner() =>
        VisitorTeamScore > HomeTeamScore ? Winner.VISITOR : Winner.HOME;

    // public Winner? GetWinner() =>
    //     HomeTeamScore > VisitorTeamScore ? Winner.HOME
    //     : VisitorTeamScore > HomeTeamScore ? Winner.VISITOR
    //     : null;
}
