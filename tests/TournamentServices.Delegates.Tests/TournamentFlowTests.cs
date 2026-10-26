using TournamentServices.Delegates.Tests.Fakes;
using TournamentServices.Domain;
using TournamentServices.Domain.Common;
using TournamentServices.Domain.Enums;
using Xunit;

namespace TournamentServices.Delegates.Tests;

// Simula un torneo completo con repositorios en memoria:
// 2 grupos de 3 equipos -> fase de grupos -> semifinales -> final -> campeón.
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

    [Fact]
    public async Task FullTournament_EndsWithAChampion()
    {
        var tournamentId = await CreateTournamentAsync();

        // 1) Fase de grupos: 3 partidos por grupo.
        var groupStage = await _delegate.GenerateMatchesAsync(tournamentId);
        Assert.True(groupStage.IsSuccess);
        Assert.Equal(6, groupStage.Value!.Count);

        // En grupos gana siempre el local: quedan a1, a2 en el grupo A y b1, b2 en el grupo B.
        await PlayAllAsync(tournamentId, homeWins: true);

        // 2) Semifinales: sembrado 1 contra 4 y 2 contra 3.
        var semifinals = await _delegate.GenerateMatchesAsync(tournamentId);
        Assert.True(semifinals.IsSuccess);
        Assert.Equal(2, semifinals.Value!.Count);
        Assert.All(semifinals.Value, m => Assert.Equal(MatchStage.PLAYOFF, m.Stage));
        Assert.Contains(semifinals.Value, m => m.HomeTeamId == "a1" && m.VisitorTeamId == "b2");
        Assert.Contains(semifinals.Value, m => m.HomeTeamId == "b1" && m.VisitorTeamId == "a2");

        await PlayAllAsync(tournamentId, homeWins: true);

        // 3) Final: a1 (local) contra b1.
        var final = await _delegate.GenerateMatchesAsync(tournamentId);
        Assert.True(final.IsSuccess);
        var finalMatch = Assert.Single(final.Value!);
        Assert.Equal(2, finalMatch.Round);
        Assert.Equal("a1", finalMatch.HomeTeamId);
        Assert.Equal("b1", finalMatch.VisitorTeamId);

        // Aún no hay campeón porque la final no se ha jugado.
        Assert.Equal(ErrorKind.Conflict, (await _delegate.GetChampionAsync(tournamentId)).Error);

        // En la final gana el visitante.
        await PlayAllAsync(tournamentId, homeWins: false);

        var champion = await _delegate.GetChampionAsync(tournamentId);
        Assert.True(champion.IsSuccess);
        Assert.Equal("b1", champion.Value!.Id);

        // Ya no se pueden generar más partidos.
        var afterFinal = await _delegate.GenerateMatchesAsync(tournamentId);
        Assert.Equal(ErrorKind.Conflict, afterFinal.Error);
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
