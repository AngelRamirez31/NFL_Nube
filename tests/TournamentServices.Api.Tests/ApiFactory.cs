using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TournamentServices.Delegates.Tests.Fakes;
using TournamentServices.Repositories;
using Microsoft.AspNetCore.TestHost;

namespace TournamentServices.Api.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITeamRepository>();
            services.AddSingleton<ITeamRepository, FakeTeamRepository>();

            services.RemoveAll<ITournamentRepository>();
            services.AddSingleton<ITournamentRepository, FakeTournamentRepository>();

            services.RemoveAll<IGroupRepository>();
            services.AddSingleton<IGroupRepository, FakeGroupRepository>();

            services.RemoveAll<IMatchRepository>();
            services.AddSingleton<IMatchRepository, FakeMatchRepository>();
        });
    }
}
