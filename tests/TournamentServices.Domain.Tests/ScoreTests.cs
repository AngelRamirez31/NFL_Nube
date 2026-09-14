using TournamentServices.Domain;
using TournamentServices.Domain.Enums;
using Xunit;

namespace TournamentServices.Domain.Tests;

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

    [Fact]
    public void GetWinner_WhenHomeScoresMore_ReturnsHome()
    {
        var score = new Score { HomeTeamScore = 24, VisitorTeamScore = 17 };

        Assert.Equal(Winner.HOME, score.GetWinner());
    }

    [Fact]
    public void GetWinner_WhenScorelessDraw_ReturnsHome()
    {
        var score = new Score { HomeTeamScore = 0, VisitorTeamScore = 0 };

        Assert.Equal(Winner.HOME, score.GetWinner());
    }

    [Theory]
    [InlineData(1, 0, Winner.HOME)]
    [InlineData(0, 1, Winner.VISITOR)]
    [InlineData(45, 44, Winner.HOME)]
    public void GetWinner_DecidesByOnePoint(int home, int visitor, Winner expected)
    {
        var score = new Score { HomeTeamScore = home, VisitorTeamScore = visitor };

        Assert.Equal(expected, score.GetWinner());
    }
}
