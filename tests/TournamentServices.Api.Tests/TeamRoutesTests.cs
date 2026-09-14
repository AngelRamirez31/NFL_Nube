using System.Net;
using System.Net.Http.Json;
using TournamentServices.Api.Dtos;
using Xunit;

namespace TournamentServices.Api.Tests;

// PERSONA 1 — ejemplo de test de integración end-to-end (levanta la API en
// memoria contra ApiFactory, sin Postgres). Persona 2/3/4: copien este
// patrón para TournamentRoutesTests, GroupRoutesTests, MatchRoutesTests.
public class TeamRoutesTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public TeamRoutesTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTeams_ReturnsOk()
    {
        var response = await _client.GetAsync("/teams");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateTeam_ThenGetById_ReturnsCreatedTeam()
    {
        var createResponse = await _client.PostAsJsonAsync("/teams", new CreateTeamDto("Eagles"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var location = createResponse.Headers.Location!.ToString();
        var getResponse = await _client.GetAsync(location);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetTeamById_WithInvalidFormat_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/teams/invalid#id!");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTeam_WithEmptyName_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/teams", new CreateTeamDto(""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // TODO: PERSONA 1 — completar según la tabla "Required Test Cases per Endpoint" (Teams) del contrato.
}
