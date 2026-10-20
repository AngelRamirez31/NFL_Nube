using FluentValidation;
using TournamentServices.Api.Common;
using TournamentServices.Domain;

namespace TournamentServices.Api.Validators;

public class MatchValidator : AbstractValidator<Match>
{
    public MatchValidator()
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

// Los marcadores negativos se rechazan con 400.
public class ScoreValidator : AbstractValidator<Score>
{
    public ScoreValidator()
    {
        RuleFor(x => x.HomeTeamScore).GreaterThanOrEqualTo(0).WithMessage("homeTeamScore must be zero or positive");
        RuleFor(x => x.VisitorTeamScore).GreaterThanOrEqualTo(0).WithMessage("visitorTeamScore must be zero or positive");
    }
}
