using TournamentServices.Delegates.Tests.Fakes;
using TournamentServices.Domain;
using TournamentServices.Domain.Common;
using TournamentServices.Domain.Enums;
using Xunit;

namespace TournamentServices.Delegates.Tests;

// Simula torneos completos con repositorios en memoria:
// fase de grupos -> playoffs de la AFC y la NFC -> Super Bowl -> campeón.
public class TournamentFlowTests
{
    private readonly FakeTournamentRepository _tournaments = new();
    private readonly FakeGroupRepository _groups = new();
    private readonly FakeMatchRepository _matches = new();
    private readonly TournamentDelegate _delegate;

    public TournamentFlowTests()
    {
        _delegate = new TournamentDelegate(_tournaments, _groups, _matches);
    }

    // Torneo chico: el grupo A es la AFC y el grupo B es la NFC, con 3 equipos cada uno.
    [Fact]
    public async Task FullTournament_EndsWithAChampion()
    {
        var tournamentId = await CreateTournamentAsync();

        // 1) Fase de grupos: 3 partidos por grupo.
        var groupStage = await _delegate.GenerateMatchesAsync(tournamentId);
        Assert.True(groupStage.IsSuccess);
        Assert.Equal(6, groupStage.Value!.Count);

        // En grupos gana siempre el local: a1 y b1 ganan su división, los demás entran como comodines.
        await PlayAllAsync(tournamentId, homeWins: true);

        // 2) Wild Card: el sembrado 1 de cada conferencia descansa y juegan el 2 contra el 3.
        var wildCard = await _delegate.GenerateMatchesAsync(tournamentId);
        Assert.True(wildCard.IsSuccess);
        Assert.Equal(2, wildCard.Value!.Count);
        Assert.All(wildCard.Value, m => Assert.Equal(MatchStage.PLAYOFF, m.Stage));
        Assert.Contains(wildCard.Value, m => m.Conference == "AFC" && m.HomeTeamId == "a2" && m.VisitorTeamId == "a3");
        Assert.Contains(wildCard.Value, m => m.Conference == "NFC" && m.HomeTeamId == "b2" && m.VisitorTeamId == "b3");

        await PlayAllAsync(tournamentId, homeWins: true);

        // 3) Campeonatos de conferencia: el sembrado 1 (que descansó) contra el que ganó.
        var championships = await _delegate.GenerateMatchesAsync(tournamentId);
        Assert.True(championships.IsSuccess);
        Assert.Equal(2, championships.Value!.Count);
        Assert.Contains(championships.Value, m => m.Conference == "AFC" && m.HomeTeamId == "a1" && m.VisitorTeamId == "a2");
        Assert.Contains(championships.Value, m => m.Conference == "NFC" && m.HomeTeamId == "b1" && m.VisitorTeamId == "b2");

        await PlayAllAsync(tournamentId, homeWins: true);

        // 4) Super Bowl: campeón de la AFC (a1) contra campeón de la NFC (b1).
        var superBowl = await _delegate.GenerateMatchesAsync(tournamentId);
        Assert.True(superBowl.IsSuccess);
        var superBowlMatch = Assert.Single(superBowl.Value!);
        Assert.Equal(MatchStage.SUPER_BOWL, superBowlMatch.Stage);
        Assert.Equal(3, superBowlMatch.Round);
        Assert.Equal("a1", superBowlMatch.HomeTeamId);
        Assert.Equal("b1", superBowlMatch.VisitorTeamId);

        // Aún no hay campeón porque el Super Bowl no se ha jugado.
        Assert.Equal(ErrorKind.Conflict, (await _delegate.GetChampionAsync(tournamentId)).Error);

        // En el Super Bowl gana el visitante.
        await PlayAllAsync(tournamentId, homeWins: false);

        var champion = await _delegate.GetChampionAsync(tournamentId);
        Assert.True(champion.IsSuccess);
        Assert.Equal("b1", champion.Value!.Id);

        // Ya no se pueden generar más partidos.
        var afterSuperBowl = await _delegate.GenerateMatchesAsync(tournamentId);
        Assert.Equal(ErrorKind.Conflict, afterSuperBowl.Error);
    }

    // Igual que la NFL real: 8 divisiones de 4 equipos (32 equipos), 7 por conferencia a playoffs.
    [Fact]
    public async Task RealNflTournament_With32Teams_PlaysLikeTheNfl()
    {
        var tournament = await _tournaments.CreateAsync(new Tournament
        {
            Name = "NFL Season",
            Format = new TournamentFormat { NumberOfGroups = 8, MaxTeamsPerGroup = 4, Type = TournamentType.NFL }
        });

        var divisions = new[] { "AFC East", "AFC North", "AFC South", "AFC West", "NFC East", "NFC North", "NFC South", "NFC West" };
        foreach (var division in divisions)
        {
            await _groups.CreateAsync(NewGroup(tournament.Id, division, Enumerable.Range(1, 4).Select(i => $"{division} {i}").ToArray()));
        }

        // Fase de grupos: 6 partidos por división.
        Assert.Equal(48, (await _delegate.GenerateMatchesAsync(tournament.Id)).Value!.Count);
        await PlayAllAsync(tournament.Id, homeWins: true);

        // Wild Card: 3 partidos por conferencia, el sembrado 1 descansa.
        var wildCard = (await _delegate.GenerateMatchesAsync(tournament.Id)).Value!;
        Assert.Equal(6, wildCard.Count);
        Assert.DoesNotContain(wildCard, m => m.HomeTeamId == "AFC East 1" || m.HomeTeamId == "NFC East 1");
        await PlayAllAsync(tournament.Id, homeWins: true);

        // Divisional: 2 por conferencia. El 1 recibe al peor sembrado que queda (el 4).
        var divisional = (await _delegate.GenerateMatchesAsync(tournament.Id)).Value!;
        Assert.Equal(4, divisional.Count);
        Assert.Contains(divisional, m => m.HomeTeamId == "AFC East 1" && m.VisitorTeamId == "AFC West 1");
        await PlayAllAsync(tournament.Id, homeWins: true);

        // Campeonatos de conferencia: 1 por conferencia.
        var championships = (await _delegate.GenerateMatchesAsync(tournament.Id)).Value!;
        Assert.Equal(2, championships.Count);
        await PlayAllAsync(tournament.Id, homeWins: true);

        // Super Bowl.
        var superBowl = Assert.Single((await _delegate.GenerateMatchesAsync(tournament.Id)).Value!);
        Assert.Equal(MatchStage.SUPER_BOWL, superBowl.Stage);
        Assert.Equal(("AFC East 1", "NFC East 1"), (superBowl.HomeTeamId, superBowl.VisitorTeamId));
        await PlayAllAsync(tournament.Id, homeWins: true);

        var champion = await _delegate.GetChampionAsync(tournament.Id);
        Assert.Equal("AFC East 1", champion.Value!.Id);
    }

    [Fact]
    public async Task Standings_AfterGroupStage_AreSortedByWins()
    {
        var tournamentId = await CreateTournamentAsync();
        await _delegate.GenerateMatchesAsync(tournamentId);
        await PlayAllAsync(tournamentId, homeWins: true);

        var result = await _delegate.GetStandingsAsync(tournamentId);

        var groupA = result.Value!.Single(g => g.GroupName == "Group A");
        Assert.Equal(new[] { "a1", "a2", "a3" }, groupA.Standings.Select(s => s.Team.Id));
        Assert.Equal(2, groupA.Standings[0].Wins);
        Assert.Equal(0, groupA.Standings[2].Wins);
    }

    // Crea un torneo de 2 grupos con 3 equipos cada uno.
    private async Task<string> CreateTournamentAsync()
    {
        var tournament = await _tournaments.CreateAsync(new Tournament
        {
            Name = "NFL Flow",
            Format = new TournamentFormat { NumberOfGroups = 2, MaxTeamsPerGroup = 4, Type = TournamentType.NFL }
        });

        await _groups.CreateAsync(NewGroup(tournament.Id, "Group A", "a1", "a2", "a3"));
        await _groups.CreateAsync(NewGroup(tournament.Id, "Group B", "b1", "b2", "b3"));

        return tournament.Id;
    }

    // Pone marcador a todos los partidos pendientes: gana el local (21-7) o el visitante (7-21).
    private async Task PlayAllAsync(string tournamentId, bool homeWins)
    {
        foreach (var match in await _matches.GetByTournamentAsync(tournamentId))
        {
            if (match.IsCompleted) continue;

            match.Score = homeWins
                ? new Score { HomeTeamScore = 21, VisitorTeamScore = 7 }
                : new Score { HomeTeamScore = 7, VisitorTeamScore = 21 };

            await _matches.UpdateAsync(match.Id, match);
        }
    }

    private static Group NewGroup(string tournamentId, string name, params string[] teamIds) => new()
    {
        Name = name,
        TournamentId = tournamentId,
        Teams = teamIds.Select(id => new Team { Id = id, Name = id }).ToList()
    };
}
