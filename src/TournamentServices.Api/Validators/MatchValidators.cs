using FluentValidation;
using TournamentServices.Api.Common;
using TournamentServices.Api.Dtos;

namespace TournamentServices.Api.Validators;

public class CreateMatchDtoValidator : AbstractValidator<CreateMatchDto>
{
    public CreateMatchDtoValidator()
    {
        RuleFor(x => x.HomeTeamId)
            .NotEmpty().WithMessage("homeTeamId is required")
            .Must(Ids.IsValid).WithMessage("homeTeamId has an invalid format");

        RuleFor(x => x.VisitorTeamId)
            .NotEmpty().WithMessage("visitorTeamId is required")
            .Must(Ids.IsValid).WithMessage("visitorTeamId has an invalid format");

        RuleFor(x => x.GroupId)
            .Must(id => id is null || Ids.IsValid(id)).WithMessage("groupId has an invalid format");
    }
}

// El contrato pide 400 (no 422) para scores negativos, por eso la regla vive
// aquí y no en el MatchDelegate (que la repite como red de seguridad).
public class UpdateScoreDtoValidator : AbstractValidator<UpdateScoreDto>
{
    public UpdateScoreDtoValidator()
    {
        RuleFor(x => x.HomeTeamScore).GreaterThanOrEqualTo(0).WithMessage("homeTeamScore must be zero or positive");
        RuleFor(x => x.VisitorTeamScore).GreaterThanOrEqualTo(0).WithMessage("visitorTeamScore must be zero or positive");
    }
}
