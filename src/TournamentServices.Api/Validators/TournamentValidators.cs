using FluentValidation;
using TournamentServices.Api.Dtos;

namespace TournamentServices.Api.Validators;

public class CreateTournamentDtoValidator : AbstractValidator<CreateTournamentDto>
{
    public CreateTournamentDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Format).NotNull().SetValidator(new TournamentFormatDtoValidator()!);
    }
}

public class UpdateTournamentDtoValidator : AbstractValidator<UpdateTournamentDto>
{
    public UpdateTournamentDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Format).NotNull().SetValidator(new TournamentFormatDtoValidator()!);
    }
}

public class TournamentFormatDtoValidator : AbstractValidator<TournamentFormatDto>
{
    public TournamentFormatDtoValidator()
    {
        RuleFor(x => x.NumberOfGroups).GreaterThan(0);
        RuleFor(x => x.MaxTeamsPerGroup).GreaterThan(0);

        // Este proyecto es solo formato NFL: no existe ROUND_ROBIN.
        RuleFor(x => x.Type)
            .Must(type => type == "NFL")
            .WithMessage("Only 'NFL' is supported as tournament type.");
    }
}
