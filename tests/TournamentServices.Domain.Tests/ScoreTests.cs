using TournamentServices.Domain;
using TournamentServices.Domain.Enums;
using Xunit;

namespace TournamentServices.Domain.Tests;

// Ejemplo ya funcionando — PERSONA 4 puede copiar este patrón
// para el resto de las pruebas de Match/Score.
public class ScoreTests
{
    [Fact]
    public void GetWinner_WhenVisitorScoresMore_ReturnsVisitor()
    {
        var score = new Score { HomeTeamScore = 10, VisitorTeamScore = 21 };

        Assert.Equal(Winner.VISITOR, score.GetWinner());
    }

    [Fact]
    public void GetWinner_WhenHomeScoresMoreOrTies_ReturnsHome()
    {
        var score = new Score { HomeTeamScore = 14, VisitorTeamScore = 14 };

        Assert.Equal(Winner.HOME, score.GetWinner());
    }

    // TODO: PERSONA 4 — agregar más casos (home gana, marcador en 0-0, etc.)
}
