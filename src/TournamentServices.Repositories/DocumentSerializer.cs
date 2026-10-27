using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using TournamentServices.Domain;

namespace TournamentServices.Repositories;

// Configuración común para convertir los documentos JSONB a objetos y viceversa.
public static class DocumentSerializer
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { IgnoreHydratedProperties } },
    };

    // Convierte un objeto a texto JSON.
    public static string To<T>(T entity) => JsonSerializer.Serialize(entity, Options);

    // Convierte texto JSON a un objeto.
    public static T From<T>(string document) => JsonSerializer.Deserialize<T>(document, Options)!;

    // Estas propiedades no se guardan en el JSONB porque viven en otras tablas.
    private static readonly Dictionary<Type, string[]> HydratedProperties = new()
    {
        [typeof(Tournament)] = [nameof(Tournament.Groups), nameof(Tournament.Matches)],
        [typeof(Match)] = [nameof(Match.HomeTeam), nameof(Match.VisitorTeam)],
    };

    // Quita del JSON las propiedades de la lista de arriba.
    private static void IgnoreHydratedProperties(JsonTypeInfo typeInfo)
    {
        if (!HydratedProperties.TryGetValue(typeInfo.Type, out var ignored)) return;

        for (var i = typeInfo.Properties.Count - 1; i >= 0; i--)
        {
            if (typeInfo.Properties[i].AttributeProvider is MemberInfo member && ignored.Contains(member.Name))
                typeInfo.Properties.RemoveAt(i);
        }
    }
}
