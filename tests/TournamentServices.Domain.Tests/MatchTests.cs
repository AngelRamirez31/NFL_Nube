using TournamentServices.Domain;
using TournamentServices.Domain.Enums;
using Xunit;

namespace TournamentServices.Domain.Tests;

// Winner e IsCompleted se calculan a partir del Score.
public class MatchTests
{
    [Fact]
    public void IsCompleted_WhenNoScoreYet_IsFalse()
    {
        var match = new Match { HomeTeamId = "team-1", VisitorTeamId = "team-2" };

        Assert.False(match.IsCompleted);
        Assert.Null(match.Winner);
    }

    [Fact]
    public void IsCompleted_WhenScoreAssigned_IsTrue()
    {
        var match = new Match
        {
            HomeTeamId = "team-1",
            VisitorTeamId = "team-2",
            Score = new Score { HomeTeamScore = 31, VisitorTeamScore = 28 }
        };

        Assert.True(match.IsCompleted);
        Assert.Equal(Winner.HOME, match.Winner);
    }

    [Fact]
    public void Winner_WhenVisitorWins_IsVisitor()
    {
        var match = new Match { Score = new Score { HomeTeamScore = 3, VisitorTeamScore = 27 } };

        Assert.Equal(Winner.VISITOR, match.Winner);
    }

    // En empate gana el equipo local (HOME). Un 0-0 también cuenta como terminado.
    [Fact]
    public void Winner_WhenTied_IsHomeAndMatchIsCompleted()
    {
        var match = new Match { Score = new Score { HomeTeamScore = 0, VisitorTeamScore = 0 } };

        Assert.Equal(Winner.HOME, match.Winner);
        Assert.True(match.IsCompleted);
    }
}
