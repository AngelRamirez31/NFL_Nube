using TournamentServices.Api.Routes;
using TournamentServices.Delegates;
using TournamentServices.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Dependency Injection — SIEMPRE apunta a las implementaciones REALES aquí.
//
// Los repos reales empiezan lanzando NotImplementedException; eso es
// normal hasta que cada quien los complete. NO registres los *Fake*
// (carpeta Repositories/Fakes) en este archivo compartido: si dos
// personas registran la misma interfaz dos veces, gana la última
// registrada y se pisan entre sí sin darse cuenta.
//
// Los Fakes son para dos usos aislados, que NO tocan este Program.cs:
//   1) Tests unitarios de tu Delegate con Moq (mockeas la interfaz).
//   2) Un WebApplicationFactory local en TUS pruebas de integración,
//      sobreescribiendo el servicio solo dentro de ese test:
//
//      var app = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
//          b.ConfigureServices(services =>
//              services.AddScoped<ITeamRepository, FakeTeamRepository>()));
//
// Así cada quien prueba su parte de forma aislada sin alterar lo que
// ven los demás al correr `dotnet run`.
// ------------------------------------------------------------

builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<ITeamDelegate, TeamDelegate>();

builder.Services.AddScoped<ITournamentRepository, TournamentRepository>();
builder.Services.AddScoped<ITournamentDelegate, TournamentDelegate>();

builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IGroupDelegate, GroupDelegate>();

builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IMatchDelegate, MatchDelegate>();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok("Services running"));

app.MapTeamRoutes();
app.MapTournamentRoutes();
app.MapGroupRoutes();
app.MapMatchRoutes();

app.Run();

// Necesario para que WebApplicationFactory<Program> funcione en los tests de integración.
public partial class Program { }
