using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Scalar.AspNetCore;
using TournamentServices.Api.Routes;
using TournamentServices.Delegates;
using TournamentServices.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNpgsqlDataSource(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection en appsettings.*.json"));

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
