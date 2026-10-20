using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace TournamentServices.Api.Tests;

public class GroupRoutesTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public GroupRoutesTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetGroups_ReturnsOk()
    {
        var response = await _client.GetAsync(
            "/tournaments/tournament-1/groups");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateGroup_ReturnsCreatedWithLocation()
    {
        var name = $"Group-{Guid.NewGuid()}";

        var response = await _client.PostAsJsonAsync(
            "/tournaments/tournament-1/groups",
            new { name });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task CreateGroup_WhenTournamentDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync(
            "/tournaments/missing-tournament/groups",
            new { name = $"Group-{Guid.NewGuid()}" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AssignTeams_WhenGroupDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.PatchAsJsonAsync(
            "/tournaments/tournament-1/groups/missing-group/teams",
            new[] { new { id = "team-1" } });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AssignTeams_WithDuplicateTeamIds_ReturnsUnprocessableEntity()
    {
        var groupId = await CreateGroupAsync();

        var response = await _client.PatchAsJsonAsync(
            $"/tournaments/tournament-1/groups/{groupId}/teams",
            new[]
            {
                new { id = "team-1" },
                new { id = "team-1" }
            });

        Assert.Equal(
            HttpStatusCode.UnprocessableEntity,
            response.StatusCode);
    }

    [Fact]
    public async Task AssignTeams_WhenCapacityWouldBeExceeded_ReturnsUnprocessableEntity()
    {
        var groupId = await CreateGroupAsync();

        var response = await _client.PatchAsJsonAsync(
            $"/tournaments/tournament-1/groups/{groupId}/teams",
            new[]
            {
                new { id = "team-1" },
                new { id = "team-2" },
                new { id = "team-3" },
                new { id = "team-4" },
                new { id = "team-5" }
            });

        Assert.Equal(
            HttpStatusCode.UnprocessableEntity,
            response.StatusCode);
    }

    [Fact]
    public async Task AssignTeams_WhenValid_ReturnsNoContent()
    {
        var groupId = await CreateGroupAsync();
        var teamId = await CreateTeamAsync();

        var response = await _client.PatchAsJsonAsync(
            $"/tournaments/tournament-1/groups/{groupId}/teams",
            new[] { new { id = teamId } });

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);
    }

    [Fact]
    public async Task AssignTeams_WhenTeamAlreadyBelongsToAnotherGroup_ReturnsUnprocessableEntity()
    {
        var firstGroupId = await CreateGroupAsync();
        var secondGroupId = await CreateGroupAsync();
        var teamId = await CreateTeamAsync();

        var firstAssignment = await _client.PatchAsJsonAsync(
            $"/tournaments/tournament-1/groups/{firstGroupId}/teams",
            new[] { new { id = teamId } });

        Assert.Equal(
            HttpStatusCode.NoContent,
            firstAssignment.StatusCode);

        var secondAssignment = await _client.PatchAsJsonAsync(
            $"/tournaments/tournament-1/groups/{secondGroupId}/teams",
            new[] { new { id = teamId } });

        Assert.Equal(
            HttpStatusCode.UnprocessableEntity,
            secondAssignment.StatusCode);
    }

    private async Task<string> CreateGroupAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/tournaments/tournament-1/groups",
            new { name = $"Group-{Guid.NewGuid()}" });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        Assert.NotNull(response.Headers.Location);

        return response.Headers.Location!
            .ToString()
            .TrimEnd('/')
            .Split('/')
            .Last();
    }

    private async Task<string> CreateTeamAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/teams",
            new { name = $"Team-{Guid.NewGuid()}" });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        Assert.NotNull(response.Headers.Location);

        return response.Headers.Location!
            .ToString()
            .TrimEnd('/')
            .Split('/')
            .Last();
    }
}