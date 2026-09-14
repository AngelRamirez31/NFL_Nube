using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Scalar.AspNetCore;
using TournamentServices.Api.Routes;
using TournamentServices.Delegates;
using TournamentServices.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Postgres — requiere que la BD esté levantada (ver README/Paso 0:
// Podman + database/db_script.sql + database/002_matches_index.sql).
// La cadena de conexión vive en appsettings.Development.json.
// ------------------------------------------------------------
builder.Services.AddNpgsqlDataSource(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection en appsettings.*.json"));

// ------------------------------------------------------------
// Dependency Injection — SIEMPRE apunta a las implementaciones REALES aquí.
//
// Los repos/delegates de Tournaments/Groups/Matches empiezan lanzando
// NotImplementedException; eso es normal hasta que cada quien los complete.
//
// NO registres los *Fake* (movidos a tests/TournamentServices.Delegates.Tests/Fakes)
// en este archivo compartido: son solo para tests aislados (Moq o
// WebApplicationFactory.ConfigureTestServices), nunca para `dotnet run`.
// ------------------------------------------------------------
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<ITeamDelegate, TeamDelegate>();

builder.Services.AddScoped<ITournamentRepository, TournamentRepository>();
builder.Services.AddScoped<ITournamentDelegate, TournamentDelegate>();

builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IGroupDelegate, GroupDelegate>();

builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IMatchDelegate, MatchDelegate>();

// ------------------------------------------------------------
// Validación, errores y serialización
// ------------------------------------------------------------
builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference(); // UI en /scalar/v1

app.MapGet("/health", () => Results.Ok("Services running"));

app.MapTeamRoutes();
app.MapTournamentRoutes();
app.MapGroupRoutes();
app.MapMatchRoutes();

app.Run();

// Necesario para que WebApplicationFactory<Program> funcione en los tests de integración.
public partial class Program { }
