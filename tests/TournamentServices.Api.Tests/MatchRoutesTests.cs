using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace TournamentServices.Api.Tests;

// Pruebas de las rutas de partidos.
// Datos de prueba: tournament-1 tiene un grupo con team-1 (Eagles) y team-2 (Cowboys).
// team-3 (Giants) no está en ningún grupo.
public class MatchRoutesTests : IClassFixture<ApiFactory>
{
    private const string Tournament = "/tournaments/tournament-1/matches";

    private readonly HttpClient _client;

    public MatchRoutesTests(ApiFactory factory) => _client = factory.CreateClient();

    // ---------- POST ----------

    [Fact]
    public async Task CreateMatch_WithTeamsInTheSameGroup_Returns201WithLocation()
    {
        var response = await _client.PostAsJsonAsync(Tournament, new { groupId = "group-with-teams", homeTeamId = "team-1", visitorTeamId = "team-2" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.StartsWith("/tournaments/tournament-1/matches/", response.Headers.Location!.ToString());

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(body.GetProperty("isCompleted").GetBoolean());
        Assert.Equal(JsonValueKind.Null, body.GetProperty("winner").ValueKind);
    }

    [Fact]
    public async Task CreateMatch_WithTheSameTeamOnBothSides_Returns422()
    {
        var response = await _client.PostAsJsonAsync(Tournament, new { groupId = (string?)null, homeTeamId = "team-1", visitorTeamId = "team-1" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateMatch_WithTeamOutsideTheTournament_Returns422()
    {
        // team-3 existe pero no pertenece a ningún grupo de tournament-1.
        var response = await _client.PostAsJsonAsync(Tournament, new { groupId = (string?)null, homeTeamId = "team-1", visitorTeamId = "team-3" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateMatch_WithUnknownGroup_Returns422()
    {
        var response = await _client.PostAsJsonAsync(Tournament, new { groupId = "group-9", homeTeamId = "team-1", visitorTeamId = "team-2" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateMatch_WithEmptyTeamId_Returns400()
    {
        var response = await _client.PostAsJsonAsync(Tournament, new { groupId = (string?)null, homeTeamId = "", visitorTeamId = "team-2" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- GET ----------

    [Fact]
    public async Task GetMatches_Returns200()
    {
        var response = await _client.GetAsync(Tournament);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMatchById_ReturnsTeamDetails()
    {
        var location = await CreateMatchAsync();

        var response = await _client.GetAsync(location);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Eagles", body.GetProperty("homeTeam").GetProperty("name").GetString());
        Assert.Equal("Cowboys", body.GetProperty("visitorTeam").GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetMatchById_WhenNotFound_Returns404()
    {
        var response = await _client.GetAsync($"{Tournament}/match-inexistente");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("invalid_id")]
    [InlineData("foo.bar")]
    [InlineData("con espacio")]
    public async Task GetMatchById_WithInvalidIdFormat_Returns400(string matchId)
    {
        var response = await _client.GetAsync($"{Tournament}/{matchId}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- PATCH score ----------

    [Fact]
    public async Task UpdateScore_ComputesWinnerAndMarksCompleted()
    {
        var location = await CreateMatchAsync();

        var response = await _client.PatchAsJsonAsync($"{location}/score", new { homeTeamScore = 31, visitorTeamScore = 28 });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("HOME", body.GetProperty("winner").GetString());
        Assert.True(body.GetProperty("isCompleted").GetBoolean());
        Assert.Equal(31, body.GetProperty("score").GetProperty("homeTeamScore").GetInt32());
    }

    [Fact]
    public async Task UpdateScore_WhenVisitorWins_ReturnsVisitor()
    {
        var location = await CreateMatchAsync();

        var response = await _client.PatchAsJsonAsync($"{location}/score", new { homeTeamScore = 10, visitorTeamScore = 24 });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("VISITOR", body.GetProperty("winner").GetString());
    }

    // Un partido de grupos puede quedar empatado: no hay ganador.
    [Fact]
    public async Task UpdateScore_WhenTied_ReturnsTieAndIsCompleted()
    {
        var location = await CreateMatchAsync();

        var response = await _client.PatchAsJsonAsync($"{location}/score", new { homeTeamScore = 17, visitorTeamScore = 17 });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("TIE", body.GetProperty("winner").GetString());
        Assert.True(body.GetProperty("isCompleted").GetBoolean());
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -7)]
    public async Task UpdateScore_WithNegativeScore_Returns400(int home, int visitor)
    {
        var location = await CreateMatchAsync();

        var response = await _client.PatchAsJsonAsync($"{location}/score", new { homeTeamScore = home, visitorTeamScore = visitor });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateScore_WhenMatchNotFound_Returns404()
    {
        var response = await _client.PatchAsJsonAsync($"{Tournament}/match-inexistente/score", new { homeTeamScore = 7, visitorTeamScore = 3 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- DELETE ----------

    [Fact]
    public async Task DeleteMatch_WhenExists_Returns204()
    {
        var location = await CreateMatchAsync();

        var response = await _client.DeleteAsync(location);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMatch_WhenNotFound_Returns404()
    {
        var response = await _client.DeleteAsync($"{Tournament}/match-inexistente");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<string> CreateMatchAsync()
    {
        var response = await _client.PostAsJsonAsync(Tournament, new { groupId = "group-with-teams", homeTeamId = "team-1", visitorTeamId = "team-2" });
        response.EnsureSuccessStatusCode();

        return response.Headers.Location!.ToString();
    }
}
