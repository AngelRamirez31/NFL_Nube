using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace TournamentServices.Api.Tests;

// Pruebas de generar partidos, tabla de posiciones y campeón.
// Se juega un torneo completo por HTTP: 2 grupos de 2 equipos (uno en la AFC y otro en la NFC)
// -> campeonatos de conferencia -> Super Bowl.
public class TournamentFlowRoutesTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public TournamentFlowRoutesTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task FullTournament_GeneratesMatchesAndReturnsTheChampion()
    {
        var tournamentId = await CreateTournamentWithGroupsAsync();
        var baseUrl = $"/tournaments/{tournamentId}";

        // Fase de grupos: 1 partido por grupo.
        var groupStage = await _client.PostAsync($"{baseUrl}/matches/generate", null);
        Assert.Equal(HttpStatusCode.Created, groupStage.StatusCode);
        Assert.Equal(2, (await groupStage.Content.ReadFromJsonAsync<JsonElement>()).GetArrayLength());

        await PlayPendingMatchesAsync(baseUrl);

        var standings = await _client.GetAsync($"{baseUrl}/standings");
        Assert.Equal(HttpStatusCode.OK, standings.StatusCode);

        // Campeonatos de conferencia: en cada conferencia juegan el campeón de división y el comodín.
        var championships = await _client.PostAsync($"{baseUrl}/matches/generate", null);
        var championshipsBody = await championships.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.Created, championships.StatusCode);
        Assert.Equal(2, championshipsBody.GetArrayLength());
        Assert.Equal("PLAYOFF", championshipsBody[0].GetProperty("stage").GetString());
        Assert.Equal("AFC", championshipsBody[0].GetProperty("conference").GetString());

        await PlayPendingMatchesAsync(baseUrl);

        // Super Bowl.
        var superBowl = await _client.PostAsync($"{baseUrl}/matches/generate", null);
        var superBowlBody = await superBowl.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, superBowlBody.GetArrayLength());
        Assert.Equal("SUPER_BOWL", superBowlBody[0].GetProperty("stage").GetString());

        var noChampionYet = await _client.GetAsync($"{baseUrl}/champion");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, noChampionYet.StatusCode);

        await PlayPendingMatchesAsync(baseUrl);

        var champion = await _client.GetAsync($"{baseUrl}/champion");
        var championBody = await champion.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.OK, champion.StatusCode);
        Assert.False(string.IsNullOrEmpty(championBody.GetProperty("name").GetString()));

        // Con el Super Bowl jugado ya no se generan más partidos.
        var afterFinal = await _client.PostAsync($"{baseUrl}/matches/generate", null);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, afterFinal.StatusCode);
    }

    // En los datos de prueba, group-1 de tournament-1 no tiene equipos.
    [Fact]
    public async Task GenerateMatches_WhenAGroupHasNoTeams_Returns422()
    {
        var response = await _client.PostAsync("/tournaments/tournament-1/matches/generate", null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GenerateMatches_WhenTournamentNotFound_Returns404()
    {
        var response = await _client.PostAsync("/tournaments/no-existe/matches/generate", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GenerateMatches_WithInvalidId_Returns400()
    {
        var response = await _client.PostAsync("/tournaments/id_invalido/matches/generate", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetStandings_WhenTournamentNotFound_Returns404()
    {
        var response = await _client.GetAsync("/tournaments/no-existe/standings");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetChampion_WhenTournamentNotFound_Returns404()
    {
        var response = await _client.GetAsync("/tournaments/no-existe/champion");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Crea un torneo con 2 grupos y 2 equipos en cada grupo. Devuelve el id del torneo.
    private async Task<string> CreateTournamentWithGroupsAsync()
    {
        var tournament = await _client.PostAsJsonAsync("/tournaments", new
        {
            name = $"Flow {Guid.NewGuid()}",
            format = new { numberOfGroups = 2, maxTeamsPerGroup = 4, type = "NFL" }
        });
        tournament.EnsureSuccessStatusCode();
        var tournamentId = (await tournament.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString()!;

        var teamIds = new List<string>();
        for (var i = 0; i < 4; i++)
        {
            var team = await _client.PostAsJsonAsync("/teams", new { name = $"Team {Guid.NewGuid()}" });
            team.EnsureSuccessStatusCode();
            teamIds.Add((await team.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString()!);
        }

        for (var g = 0; g < 2; g++)
        {
            var group = await _client.PostAsJsonAsync($"/tournaments/{tournamentId}/groups", new { name = $"Group {g + 1}" });
            group.EnsureSuccessStatusCode();
            var groupId = (await group.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString()!;

            var teams = teamIds.Skip(g * 2).Take(2).Select(id => new { id });
            var assign = await _client.PatchAsJsonAsync($"/tournaments/{tournamentId}/groups/{groupId}/teams", teams);
            assign.EnsureSuccessStatusCode();
        }

        return tournamentId;
    }

    // Pone marcador 21-14 (gana el local) a todos los partidos que aún no se juegan.
    private async Task PlayPendingMatchesAsync(string baseUrl)
    {
        var matches = await _client.GetFromJsonAsync<JsonElement>($"{baseUrl}/matches");

        foreach (var match in matches.EnumerateArray())
        {
            if (match.GetProperty("isCompleted").GetBoolean()) continue;

            var id = match.GetProperty("id").GetString();
            var response = await _client.PatchAsJsonAsync($"{baseUrl}/matches/{id}/score", new { homeTeamScore = 21, visitorTeamScore = 14 });
            response.EnsureSuccessStatusCode();
        }
    }
}
