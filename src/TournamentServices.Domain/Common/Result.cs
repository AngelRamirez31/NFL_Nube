namespace TournamentServices.Domain.Common;

// Equivalente a std::expected<T, std::string> del proyecto de referencia en C++.
// NotFound -> 404, Validation -> 400, Conflict -> 422 (mapeo en Api/Common/ResultExtensions.cs).
public enum ErrorKind
{
    None,
    NotFound,
    Validation,
    Conflict
}

public sealed record Result<T>(T? Value, ErrorKind Error, string? Message)
{
    public bool IsSuccess => Error == ErrorKind.None;

    public static Result<T> Ok(T value) => new(value, ErrorKind.None, null);
    public static Result<T> NotFound(string message) => new(default, ErrorKind.NotFound, message);
    public static Result<T> Conflict(string message) => new(default, ErrorKind.Conflict, message);
    public static Result<T> Invalid(string message) => new(default, ErrorKind.Validation, message);
}

// Para operaciones que no devuelven un valor (p.ej. Delete, AssignTeams).
public readonly record struct Unit
{
    public static readonly Unit Value = new();
}
