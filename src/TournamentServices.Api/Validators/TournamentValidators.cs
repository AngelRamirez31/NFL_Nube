using FluentValidation;
using TournamentServices.Domain;

namespace TournamentServices.Api.Validators;

public class TournamentValidator : AbstractValidator<Tournament>
{
    public TournamentValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Format).NotNull().SetValidator(new TournamentFormatValidator()!);
    }
}

public class TournamentFormatValidator : AbstractValidator<TournamentFormat>
{
    public TournamentFormatValidator()
    {
        RuleFor(x => x.NumberOfGroups).GreaterThan(0);
        RuleFor(x => x.MaxTeamsPerGroup).GreaterThan(0);

        // Solo se acepta el formato NFL.
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Only 'NFL' is supported as tournament type.");
    }
}
