using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TournamentServices.Delegates.Tests.Fakes;
using TournamentServices.Repositories;
using Microsoft.AspNetCore.TestHost;

namespace TournamentServices.Api.Tests;

// Levanta la API en memoria SIN Postgres: reemplaza los repos reales por
// los fakes (movidos a Delegates.Tests/Fakes) para que las rutas se
// verifiquen en milisegundos. Persona 2/3/4 agreguen aquí el Remove/Add
// del repo que les toque cuando escriban sus propios *RoutesTests.
//
// OJO: Singleton, no Scoped — con Scoped los datos se pierden entre
// requests y un test tipo "CreateTeam_ThenGetById" fallaría.
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
        });
    }
}
