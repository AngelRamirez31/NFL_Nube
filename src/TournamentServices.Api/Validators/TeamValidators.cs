using FluentValidation;
using TournamentServices.Domain;

namespace TournamentServices.Api.Validators;

public class TeamValidator : AbstractValidator<Team>
{
    public TeamValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
