using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TournamentServices.Api.Dtos;
using Xunit;

namespace TournamentServices.Api.Tests;

// Tests de contrato (código HTTP, Location, forma del JSON) contra ApiFactory.
// Datos precargados por los fakes: tournament-1 -> group-1 -> team-1 (Eagles),
// team-2 (Cowboys); team-3 (Giants) no está en ningún grupo del torneo.
public class MatchRoutesTests : IClassFixture<ApiFactory>
{
    private const string Tournament = "/tournaments/tournament-1/matches";

    private readonly HttpClient _client;

    public MatchRoutesTests(ApiFactory factory) => _client = factory.CreateClient();

    // ---------- POST ----------

    [Fact]
    public async Task CreateMatch_WithTeamsInTheSameGroup_Returns201WithLocation()
    {
        var response = await _client.PostAsJsonAsync(Tournament, new CreateMatchDto("group-1", "team-1", "team-2"));

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
        var response = await _client.PostAsJsonAsync(Tournament, new CreateMatchDto(null, "team-1", "team-1"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateMatch_WithTeamOutsideTheTournament_Returns422()
    {
        // team-3 existe pero no pertenece a ningún grupo de tournament-1.
        var response = await _client.PostAsJsonAsync(Tournament, new CreateMatchDto(null, "team-1", "team-3"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateMatch_WithUnknownGroup_Returns422()
    {
        var response = await _client.PostAsJsonAsync(Tournament, new CreateMatchDto("group-9", "team-1", "team-2"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateMatch_WithEmptyTeamId_Returns400()
    {
        var response = await _client.PostAsJsonAsync(Tournament, new CreateMatchDto(null, "", "team-2"));

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

    // Ojo con el dato de prueba: un id como "invalid#id!" no sirve aquí porque
    // el cliente HTTP trata el # como inicio del fragmento y al servidor solo
    // le llega "invalid", que sí cumple el regex.
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

        var response = await _client.PatchAsJsonAsync($"{location}/score", new UpdateScoreDto(31, 28));
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

        var response = await _client.PatchAsJsonAsync($"{location}/score", new UpdateScoreDto(10, 24));
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("VISITOR", body.GetProperty("winner").GetString());
    }

    // Empate = HOME (pendiente de confirmar con el profesor, ver Domain/Score.cs).
    [Fact]
    public async Task UpdateScore_WhenTied_ReturnsHomeAndIsCompleted()
    {
        var location = await CreateMatchAsync();

        var response = await _client.PatchAsJsonAsync($"{location}/score", new UpdateScoreDto(17, 17));
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("HOME", body.GetProperty("winner").GetString());
        Assert.True(body.GetProperty("isCompleted").GetBoolean());
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -7)]
    public async Task UpdateScore_WithNegativeScore_Returns400(int home, int visitor)
    {
        var location = await CreateMatchAsync();

        var response = await _client.PatchAsJsonAsync($"{location}/score", new UpdateScoreDto(home, visitor));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateScore_WhenMatchNotFound_Returns404()
    {
        var response = await _client.PatchAsJsonAsync($"{Tournament}/match-inexistente/score", new UpdateScoreDto(7, 3));

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
        var response = await _client.PostAsJsonAsync(Tournament, new CreateMatchDto("group-1", "team-1", "team-2"));
        response.EnsureSuccessStatusCode();

        return response.Headers.Location!.ToString();
    }
}
