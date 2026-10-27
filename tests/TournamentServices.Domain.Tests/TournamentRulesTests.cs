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

    // ---------- GetConferences ----------

    // Los grupos se ordenan por nombre: la primera mitad es la AFC y la segunda la NFC.
    [Fact]
    public void GetConferences_SplitsGroupsByNameIntoAfcAndNfc()
    {
        var groups = new[]
        {
            NewGroup("NFC East"), NewGroup("AFC West"), NewGroup("NFC North"), NewGroup("AFC East")
        };

        var conferences = TournamentRules.GetConferences(groups);

        Assert.Equal("AFC", conferences[0].Name);
        Assert.Equal(new[] { "AFC East", "AFC West" }, conferences[0].Groups.Select(g => g.Name));
        Assert.Equal("NFC", conferences[1].Name);
        Assert.Equal(new[] { "NFC East", "NFC North" }, conferences[1].Groups.Select(g => g.Name));
    }

    // ---------- GetPlayoffSeeds ----------

    // 4 divisiones: los 4 campeones son los sembrados 1-4 y los 3 comodines son los sembrados 5-7.
    // Un campeón de división con peor récord que un comodín sigue estando arriba de él (como en la NFL).
    [Fact]
    public void GetPlayoffSeeds_FourDivisions_DivisionWinnersThenThreeWildCards()
    {
        var east = NewGroup("AFC East", "e1", "e2", "e3", "e4");
        var north = NewGroup("AFC North", "n1", "n2", "n3");
        var south = NewGroup("AFC South", "s1", "s2", "s3");
        var west = NewGroup("AFC West", "w1", "w2", "w3");

        var matches = new List<Match>();
        matches.AddRange(HomeAlwaysWins(east));    // e1 3-0, e2 2-1
        matches.AddRange(HomeAlwaysWins(north));   // n1 2-0, n2 1-1
        matches.AddRange(HomeAlwaysWins(south));   // s1 2-0, s2 1-1
        matches.AddRange(new[]                     // en el oeste todos quedan 1-1; gana w1 por diferencia
        {
            Played("w1", "w2", 21, 0),
            Played("w2", "w3", 21, 0),
            Played("w3", "w1", 10, 7)
        });

        var seeds = TournamentRules.GetPlayoffSeeds(new[] { east, north, south, west }, matches);

        Assert.Equal(new[] { "e1", "n1", "s1", "w1", "e2", "n2", "s2" }, seeds.Select(t => t.Id));
    }

    // Con un solo grupo en la conferencia, el campeón es el 1 y los demás entran como comodines.
    [Fact]
    public void GetPlayoffSeeds_OneDivision_WinnerFirstThenWildCards()
    {
        var group = NewGroup("AFC", "t1", "t2", "t3");

        var seeds = TournamentRules.GetPlayoffSeeds(new[] { group }, HomeAlwaysWins(group));

        Assert.Equal(new[] { "t1", "t2", "t3" }, seeds.Select(t => t.Id));
    }

    // ---------- GetByes ----------

    // 7 equipos: descansa solo el 1 (NFL actual). 6 equipos: descansan el 1 y el 2 (NFL antes de 2020).
    [Theory]
    [InlineData(7, 1)]
    [InlineData(6, 2)]
    [InlineData(4, 0)]
    [InlineData(3, 1)]
    [InlineData(2, 0)]
    public void GetByes_ReturnsHowManyTopSeedsRest(int seedCount, int expectedByes)
    {
        Assert.Equal(expectedByes, TournamentRules.GetByes(seedCount));
    }

    // ---------- CreatePlayoffRound ----------

    // El sembrado 1 juega contra el 4 y el 2 contra el 3. El mejor sembrado es local.
    [Fact]
    public void CreatePlayoffRound_PairsBestAgainstWorst()
    {
        var seeds = new[] { "s1", "s2", "s3", "s4" }.Select(id => new Team { Id = id }).ToList();

        var matches = TournamentRules.CreatePlayoffRound(TournamentId, seeds, round: 1, conference: "AFC");

        Assert.Equal(2, matches.Count);
        Assert.Equal(("s1", "s4"), (matches[0].HomeTeamId, matches[0].VisitorTeamId));
        Assert.Equal(("s2", "s3"), (matches[1].HomeTeamId, matches[1].VisitorTeamId));
        Assert.All(matches, m =>
        {
            Assert.Equal(MatchStage.PLAYOFF, m.Stage);
            Assert.Equal(1, m.Round);
            Assert.Equal("AFC", m.Conference);
            Assert.Null(m.GroupId);
        });
    }

    // ---------- CreateNextPlayoffRound ----------

    // Ronda de Wild Card con las 8 divisiones de la NFL: en cada conferencia descansa el 1
    // y se juega 2 vs 7, 3 vs 6 y 4 vs 5.
    [Fact]
    public void CreateNextPlayoffRound_WildCardRound_TopSeedRestsAndTheRestPlay()
    {
        var groups = NflDivisions();
        var matches = groups.SelectMany(HomeAlwaysWins).ToList();

        var wildCard = TournamentRules.CreateNextPlayoffRound(TournamentId, groups, matches);

        Assert.Equal(6, wildCard.Count);
        Assert.All(wildCard, m => Assert.Equal(1, m.Round));

        var afc = wildCard.Where(m => m.Conference == "AFC").Select(m => (m.HomeTeamId, m.VisitorTeamId));
        Assert.Equal(new[]
        {
            ("AFC North 1", "AFC South 2"),   // 2 vs 7
            ("AFC South 1", "AFC North 2"),   // 3 vs 6
            ("AFC West 1", "AFC East 2")      // 4 vs 5
        }, afc);

        // El sembrado 1 de cada conferencia descansa.
        Assert.DoesNotContain(wildCard, m => m.HomeTeamId == "AFC East 1" || m.VisitorTeamId == "AFC East 1");
        Assert.DoesNotContain(wildCard, m => m.HomeTeamId == "NFC East 1" || m.VisitorTeamId == "NFC East 1");
    }

    // Ronda Divisional: se vuelve a sembrar y el 1 juega contra el peor sembrado que sigue vivo.
    [Fact]
    public void CreateNextPlayoffRound_DivisionalRound_TopSeedPlaysLowestRemainingSeed()
    {
        var groups = NflDivisions();
        var matches = groups.SelectMany(HomeAlwaysWins).ToList();

        // En el Wild Card ganan todos los visitantes: pasan los sembrados 5, 6 y 7.
        matches.AddRange(Play(TournamentRules.CreateNextPlayoffRound(TournamentId, groups, matches), homeWins: false));

        var divisional = TournamentRules.CreateNextPlayoffRound(TournamentId, groups, matches);

        Assert.Equal(4, divisional.Count);
        Assert.All(divisional, m => Assert.Equal(2, m.Round));

        var afc = divisional.Where(m => m.Conference == "AFC").Select(m => (m.HomeTeamId, m.VisitorTeamId));
        Assert.Equal(new[]
        {
            ("AFC East 1", "AFC South 2"),   // 1 vs 7
            ("AFC East 2", "AFC North 2")    // 5 vs 6
        }, afc);
    }

    // Cuando la AFC y la NFC tienen campeón se genera el Super Bowl. Es local el de mejor récord.
    [Fact]
    public void CreateNextPlayoffRound_WhenBothConferencesHaveAChampion_CreatesTheSuperBowl()
    {
        var afc = NewGroup("AFC", "a1", "a2");
        var nfc = NewGroup("NFC", "n1", "n2");
        var groups = new[] { afc, nfc };
        var matches = new List<Match> { Played("a1", "a2", 21, 0), Played("n1", "n2", 21, 0) };

        // Campeonatos de conferencia: a1 gana la AFC y n2 gana la NFC.
        var championships = TournamentRules.CreateNextPlayoffRound(TournamentId, groups, matches);
        Assert.Equal(2, championships.Count);
        championships[0].Score = new Score { HomeTeamScore = 24, VisitorTeamScore = 10 };
        championships[1].Score = new Score { HomeTeamScore = 10, VisitorTeamScore = 24 };
        matches.AddRange(championships);

        var superBowl = Assert.Single(TournamentRules.CreateNextPlayoffRound(TournamentId, groups, matches));

        Assert.Equal(MatchStage.SUPER_BOWL, superBowl.Stage);
        Assert.Equal(2, superBowl.Round);
        Assert.Null(superBowl.Conference);
        Assert.Equal(("a1", "n2"), (superBowl.HomeTeamId, superBowl.VisitorTeamId));
    }

    // Después del Super Bowl ya no hay nada que generar.
    [Fact]
    public void CreateNextPlayoffRound_AfterTheSuperBowl_ReturnsEmpty()
    {
        var groups = new[] { NewGroup("AFC", "a1", "a2"), NewGroup("NFC", "n1", "n2") };
        var matches = new List<Match> { SuperBowl("a1", "n1", 21, 0, round: 2) };

        Assert.Empty(TournamentRules.CreateNextPlayoffRound(TournamentId, groups, matches));
    }

    // ---------- GetChampion ----------

    [Fact]
    public void GetChampion_WithoutPlayoffs_ReturnsNull()
    {
        var group = NewGroup("group-a", "t1", "t2");

        Assert.Null(TournamentRules.GetChampion(new[] { group }, new[] { Played("t1", "t2", 7, 0) }));
    }

    [Fact]
    public void GetChampion_WhenSuperBowlIsNotPlayed_ReturnsNull()
    {
        var group = NewGroup("group-a", "t1", "t2");
        var superBowl = new Match { HomeTeamId = "t1", VisitorTeamId = "t2", Stage = MatchStage.SUPER_BOWL, Round = 2 };

        Assert.Null(TournamentRules.GetChampion(new[] { group }, new[] { superBowl }));
    }

    // Un campeonato de conferencia no es el Super Bowl, así que todavía no hay campeón.
    [Fact]
    public void GetChampion_AfterConferenceChampionships_ReturnsNull()
    {
        var group = NewGroup("group-a", "t1", "t2");
        var championship = Playoff("t1", "t2", 21, 0, round: 1);

        Assert.Null(TournamentRules.GetChampion(new[] { group }, new[] { championship }));
    }

    [Fact]
    public void GetChampion_ReturnsTheWinnerOfTheSuperBowl()
    {
        var afc = NewGroup("AFC", "a1", "a2");
        var nfc = NewGroup("NFC", "n1", "n2");
        var matches = new List<Match>
        {
            Playoff("a1", "a2", 21, 0, round: 1),
            Playoff("n1", "n2", 21, 0, round: 1),
            SuperBowl("a1", "n1", 3, 17, round: 2)   // gana el visitante
        };

        var champion = TournamentRules.GetChampion(new[] { afc, nfc }, matches);

        Assert.Equal("n1", champion!.Id);
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

    private static Match SuperBowl(string home, string visitor, int homeScore, int visitorScore, int round)
    {
        var match = Played(home, visitor, homeScore, visitorScore);
        match.Stage = MatchStage.SUPER_BOWL;
        match.Round = round;
        return match;
    }

    // Genera los partidos del grupo y en todos gana el local 21-7.
    private static IEnumerable<Match> HomeAlwaysWins(Group group) =>
        Play(TournamentRules.CreateGroupMatches(TournamentId, group), homeWins: true);

    // Pone marcador a los partidos: gana el local (21-7) o el visitante (7-21).
    private static List<Match> Play(IEnumerable<Match> matches, bool homeWins) =>
        matches.Select(m =>
        {
            m.Score = homeWins
                ? new Score { HomeTeamScore = 21, VisitorTeamScore = 7 }
                : new Score { HomeTeamScore = 7, VisitorTeamScore = 21 };
            return m;
        }).ToList();

    // Las 8 divisiones de la NFL con 4 equipos cada una (ids como "AFC East 1").
    private static List<Group> NflDivisions() =>
        new[] { "AFC East", "AFC North", "AFC South", "AFC West", "NFC East", "NFC North", "NFC South", "NFC West" }
            .Select(name => NewGroup(name, Enumerable.Range(1, 4).Select(i => $"{name} {i}").ToArray()))
            .ToList();
}
