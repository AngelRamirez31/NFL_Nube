using FluentValidation;

namespace TournamentServices.Api.Common;

// Endpoint filter genérico: corre el IValidator<T> registrado para el tipo
// del body y devuelve 400 con ProblemDetails si falla. Uso:
//   group.MapPost("/", handler).AddEndpointFilter<ValidationFilter<CreateTeamDto>>();
public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var dto = context.Arguments.OfType<T>().FirstOrDefault();
        if (dto is null) return await next(context);

        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null) return await next(context);

        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            return Results.ValidationProblem(result.ToDictionary());
        }

        return await next(context);
    }
}
