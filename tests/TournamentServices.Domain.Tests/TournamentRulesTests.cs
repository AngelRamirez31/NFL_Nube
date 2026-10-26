using TournamentServices.Domain;
using TournamentServices.Domain.Enums;
using Xunit;

namespace TournamentServices.Domain.Tests;

public class TournamentRulesTests
{
    private const string TournamentId = "tournament-1";

    // ---------- CreateGroupMatches ----------

    // 4 equipos todos contra todos = 6 partidos.
    [Fact]
    public void CreateGroupMatches_WithFourTeams_CreatesSixMatches()
    {
        var group = NewGroup("group-a", "t1", "t2", "t3", "t4");

        var matches = TournamentRules.CreateGroupMatches(TournamentId, group);

        Assert.Equal(6, matches.Count);
        Assert.All(matches, m =>
        {
            Assert.NotEqual(m.HomeTeamId, m.VisitorTeamId);
            Assert.Equal("group-a", m.GroupId);
            Assert.Equal(MatchStage.GROUP, m.Stage);
            Assert.False(m.IsCompleted);
        });
    }

    // Cada equipo juega contra cada uno de los demás una sola vez.
    [Fact]
    public void CreateGroupMatches_EveryPairPlaysOnce()
    {
        var group = NewGroup("group-a", "t1", "t2", "t3");

        var pairs = TournamentRules.CreateGroupMatches(TournamentId, group)
            .Select(m => string.Join("-", new[] { m.HomeTeamId, m.VisitorTeamId }.Order()))
            .ToList();

        Assert.Equal(new[] { "t1-t2", "t1-t3", "t2-t3" }, pairs.Order());
    }

    // ---------- CalculateStandings ----------

    [Fact]
    public void CalculateStandings_CountsWinsLossesAndPoints()
    {
        var group = NewGroup("group-a", "t1", "t2");
        var matches = new[] { Played("t1", "t2", 10, 24) };

        var standings = TournamentRules.CalculateStandings(group, matches);

        var first = standings[0];
        Assert.Equal("t2", first.Team.Id);
        Assert.Equal(1, first.Played);
        Assert.Equal(1, first.Wins);
        Assert.Equal(0, first.Losses);
        Assert.Equal(24, first.PointsFor);
        Assert.Equal(10, first.PointsAgainst);
        Assert.Equal(14, first.PointDifference);

        Assert.Equal("t1", standings[1].Team.Id);
        Assert.Equal(1, standings[1].Losses);
    }

    // Con las mismas victorias, decide la diferencia de puntos.
    [Fact]
    public void CalculateStandings_TieOnWins_UsesPointDifference()
    {
        var group = NewGroup("group-a", "t1", "t2", "t3");
        var matches = new[]
        {
            Played("t1", "t2", 30, 0),   // t1 gana por 30
            Played("t2", "t3", 7, 0),    // t2 gana por 7
            Played("t3", "t1", 14, 10)   // t3 gana por 4
        };

        var standings = TournamentRules.CalculateStandings(group, matches);

        Assert.Equal(new[] { "t1", "t3", "t2" }, standings.Select(s => s.Team.Id));
    }

    // Los partidos sin marcador y los de playoffs no cuentan para la tabla.
    [Fact]
    public void CalculateStandings_IgnoresPendingAndPlayoffMatches()
    {
        var group = NewGroup("group-a", "t1", "t2");
        var playoff = Played("t1", "t2", 21, 0);
        playoff.Stage = MatchStage.PLAYOFF;
        var matches = new[]
        {
            new Match { HomeTeamId = "t1", VisitorTeamId = "t2", Stage = MatchStage.GROUP },
            playoff
        };

        var standings = TournamentRules.CalculateStandings(group, matches);

        Assert.All(standings, s => Assert.Equal(0, s.Played));
    }

    // ---------- GetPlayoffSeeds ----------

    // Pasan 2 por grupo; primero los líderes y después los segundos lugares.
    [Fact]
    public void GetPlayoffSeeds_TakesTopTwoOfEachGroup_LeadersFirst()
    {
        var groupA = NewGroup("group-a", "a1", "a2", "a3");
        var groupB = NewGroup("group-b", "b1", "b2", "b3");
        var matches = new List<Match>();
        matches.AddRange(HomeAlwaysWins(groupA));
        matches.AddRange(HomeAlwaysWins(groupB));

        var seeds = TournamentRules.GetPlayoffSeeds(new[] { groupA, groupB }, matches);

        Assert.Equal(new[] { "a1", "b1", "a2", "b2" }, seeds.Select(t => t.Id));
    }

    // 3 grupos dan 6 clasificados, pero la llave usa 4: los 3 líderes y el mejor segundo lugar.
    [Fact]
    public void GetPlayoffSeeds_WithThreeGroups_KeepsFourTeams()
    {
        var groups = new[]
        {
            NewGroup("group-a", "a1", "a2"),
            NewGroup("group-b", "b1", "b2"),
            NewGroup("group-c", "c1", "c2")
        };
        var matches = new[]
        {
            Played("a1", "a2", 21, 20),  // a2 pierde por 1: mejor segundo lugar
            Played("b1", "b2", 21, 0),
            Played("c1", "c2", 21, 0)
        };

        var seeds = TournamentRules.GetPlayoffSeeds(groups, matches);

        Assert.Equal(4, seeds.Count);
        Assert.Equal("a2", seeds[3].Id);
    }

    // ---------- CreatePlayoffRound ----------

    // El sembrado 1 juega contra el 4 y el 2 contra el 3. El mejor sembrado es local.
    [Fact]
    public void CreatePlayoffRound_PairsBestAgainstWorst()
    {
        var seeds = new[] { "s1", "s2", "s3", "s4" }.Select(id => new Team { Id = id }).ToList();

        var matches = TournamentRules.CreatePlayoffRound(TournamentId, seeds, round: 1);

        Assert.Equal(2, matches.Count);
        Assert.Equal(("s1", "s4"), (matches[0].HomeTeamId, matches[0].VisitorTeamId));
        Assert.Equal(("s2", "s3"), (matches[1].HomeTeamId, matches[1].VisitorTeamId));
        Assert.All(matches, m =>
        {
            Assert.Equal(MatchStage.PLAYOFF, m.Stage);
            Assert.Equal(1, m.Round);
            Assert.Null(m.GroupId);
        });
    }

    // ---------- GetChampion ----------

    [Fact]
    public void GetChampion_WithoutPlayoffs_ReturnsNull()
    {
        var group = NewGroup("group-a", "t1", "t2");

        Assert.Null(TournamentRules.GetChampion(new[] { group }, new[] { Played("t1", "t2", 7, 0) }));
    }

    [Fact]
    public void GetChampion_WhenFinalIsNotPlayed_ReturnsNull()
    {
        var group = NewGroup("group-a", "t1", "t2");
        var final = new Match { HomeTeamId = "t1", VisitorTeamId = "t2", Stage = MatchStage.PLAYOFF, Round = 1 };

        Assert.Null(TournamentRules.GetChampion(new[] { group }, new[] { final }));
    }

    [Fact]
    public void GetChampion_ReturnsTheWinnerOfTheFinal()
    {
        var group = NewGroup("group-a", "t1", "t2", "t3", "t4");
        var matches = new List<Match>
        {
            Playoff("t1", "t4", 21, 0, round: 1),
            Playoff("t2", "t3", 21, 0, round: 1),
            Playoff("t1", "t2", 3, 17, round: 2)   // final: gana el visitante
        };

        var champion = TournamentRules.GetChampion(new[] { group }, matches);

        Assert.Equal("t2", champion!.Id);
    }

    // ---------- helpers ----------

    private static Group NewGroup(string id, params string[] teamIds) => new()
    {
        Id = id,
        Name = id,
        TournamentId = TournamentId,
        Teams = teamIds.Select(t => new Team { Id = t, Name = t }).ToList()
    };

    private static Match Played(string home, string visitor, int homeScore, int visitorScore) => new()
    {
        TournamentId = TournamentId,
        HomeTeamId = home,
        VisitorTeamId = visitor,
        Stage = MatchStage.GROUP,
        Score = new Score { HomeTeamScore = homeScore, VisitorTeamScore = visitorScore }
    };

    private static Match Playoff(string home, string visitor, int homeScore, int visitorScore, int round)
    {
        var match = Played(home, visitor, homeScore, visitorScore);
        match.Stage = MatchStage.PLAYOFF;
        match.Round = round;
        return match;
    }

    // Genera los partidos del grupo y en todos gana el local 21-7.
    private static IEnumerable<Match> HomeAlwaysWins(Group group) =>
        TournamentRules.CreateGroupMatches(TournamentId, group).Select(m =>
        {
            m.Score = new Score { HomeTeamScore = 21, VisitorTeamScore = 7 };
            return m;
        });
}
