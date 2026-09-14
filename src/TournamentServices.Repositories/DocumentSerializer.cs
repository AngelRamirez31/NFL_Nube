using System.Text.Json;
using System.Text.Json.Serialization;

namespace TournamentServices.Repositories;

// Un único lugar donde se decide cómo se serializa/deserializa el JSONB
// de cada tabla. Todos los repos usan esto — no crear otro JsonSerializerOptions.
public static class DocumentSerializer
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string To<T>(T entity) => JsonSerializer.Serialize(entity, Options);

    public static T From<T>(string document) => JsonSerializer.Deserialize<T>(document, Options)!;
}
