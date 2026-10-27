namespace TournamentServices.Domain.Common;

// Resultado de una operación: el valor si salió bien, o el error si falló.
// NotFound -> 404, Validation -> 400, Conflict -> 422.
public enum ErrorKind
{
    None,
    NotFound,
    Validation,
    Conflict
}

public sealed record Result<T>(T? Value, ErrorKind Error, string? Message)
{
    // Es true cuando no hubo ningún error.
    public bool IsSuccess => Error == ErrorKind.None;

    // Operación exitosa con su valor.
    public static Result<T> Ok(T value) => new(value, ErrorKind.None, null);
    // No se encontró lo que se buscaba.
    public static Result<T> NotFound(string message) => new(default, ErrorKind.NotFound, message);
    // La operación choca con una regla del torneo.
    public static Result<T> Conflict(string message) => new(default, ErrorKind.Conflict, message);
    // Los datos que llegaron no son válidos.
    public static Result<T> Invalid(string message) => new(default, ErrorKind.Validation, message);
}

// Para operaciones que no devuelven valor (por ejemplo Delete).
public readonly record struct Unit
{
    public static readonly Unit Value = new();
}
