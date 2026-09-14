using TournamentServices.Domain.Common;

namespace TournamentServices.Api.Common;

// Traducción única de Result<T> -> respuesta HTTP. Úsala en todas las rutas
// en vez de escribir el switch a mano en cada endpoint.
public static class ResultExtensions
{
    public static IResult ToHttp<T>(this Result<T> result, Func<T, object>? map = null) => result.Error switch
    {
        ErrorKind.None => Results.Ok(map is null ? result.Value : map(result.Value!)),
        ErrorKind.NotFound => Results.Problem(result.Message, statusCode: StatusCodes.Status404NotFound),
        ErrorKind.Conflict => Results.Problem(result.Message, statusCode: StatusCodes.Status422UnprocessableEntity),
        ErrorKind.Validation => Results.Problem(result.Message, statusCode: StatusCodes.Status400BadRequest),
        _ => Results.Problem(statusCode: StatusCodes.Status500InternalServerError),
    };

    // Para POST: 201 Created con Location, o el error que corresponda.
    public static IResult ToCreatedHttp<T>(this Result<T> result, Func<T, string> locationOf, Func<T, object>? map = null) => result.Error switch
    {
        ErrorKind.None => Results.Created(locationOf(result.Value!), map is null ? result.Value : map(result.Value!)),
        ErrorKind.NotFound => Results.Problem(result.Message, statusCode: StatusCodes.Status404NotFound),
        ErrorKind.Conflict => Results.Problem(result.Message, statusCode: StatusCodes.Status422UnprocessableEntity),
        ErrorKind.Validation => Results.Problem(result.Message, statusCode: StatusCodes.Status400BadRequest),
        _ => Results.Problem(statusCode: StatusCodes.Status500InternalServerError),
    };

    // Para DELETE / PATCH sin cuerpo de respuesta: 204, o el error que corresponda.
    public static IResult ToNoContentHttp(this Result<Unit> result) => result.Error switch
    {
        ErrorKind.None => Results.NoContent(),
        ErrorKind.NotFound => Results.Problem(result.Message, statusCode: StatusCodes.Status404NotFound),
        ErrorKind.Conflict => Results.Problem(result.Message, statusCode: StatusCodes.Status422UnprocessableEntity),
        ErrorKind.Validation => Results.Problem(result.Message, statusCode: StatusCodes.Status400BadRequest),
        _ => Results.Problem(statusCode: StatusCodes.Status500InternalServerError),
    };
}
