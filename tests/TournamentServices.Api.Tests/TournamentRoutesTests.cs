using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TournamentServices.Api.Dtos;
using Xunit;

namespace TournamentServices.Api.Tests;

public class TournamentRoutesTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public TournamentRoutesTests(ApiFactory factory) => _client = factory.CreateClient();

    private static TournamentFormatDto Format(string type = "NFL") => new(4, 2, type);

    [Fact]
    public async Task GetTournaments_ReturnsOk()
    {
        var response = await _client.GetAsync("/tournaments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateTournament_ThenGetById_ReturnsCreatedTournament()
    {
        var location = await CreateAsync();

        var response = await _client.GetAsync(location);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("NFL", body.GetProperty("format").GetProperty("type").GetString());
    }

    [Fact]
    public async Task CreateTournament_WithInvalidType_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/tournaments",
            new CreateTournamentDto($"T-{Guid.NewGuid()}", Format("ROUND_ROBIN")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTournamentById_WhenNotFound_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/tournaments/torneo-inexistente");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // El caso clave del PATCH: mandar solo el nombre no debe borrar el format.
    [Fact]
    public async Task PatchTournament_WithNameOnly_KeepsFormatUntouched()
    {
        var location = await CreateAsync();

        var response = await _client.PatchAsJsonAsync(
            location,
            new PatchTournamentDto("Nombre nuevo", null));
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Nombre nuevo", body.GetProperty("name").GetString());
        Assert.Equal(2, body.GetProperty("format").GetProperty("numberOfGroups").GetInt32());
        Assert.Equal(4, body.GetProperty("format").GetProperty("maxTeamsPerGroup").GetInt32());
    }

    [Fact]
    public async Task DeleteTournament_WhenExists_ReturnsNoContent()
    {
        var location = await CreateAsync();

        var response = await _client.DeleteAsync(location);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<string> CreateAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/tournaments",
            new CreateTournamentDto($"T-{Guid.NewGuid()}", Format()));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return response.Headers.Location!.ToString();
    }
}
