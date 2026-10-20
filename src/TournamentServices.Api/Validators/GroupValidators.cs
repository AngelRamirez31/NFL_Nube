using FluentValidation;
using TournamentServices.Api.Common;
using TournamentServices.Domain;

namespace TournamentServices.Api.Validators;

public class GroupValidator
    : AbstractValidator<Group>
{
    public GroupValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}

public class AssignTeamsValidator
    : AbstractValidator<List<Team>>
{
    public AssignTeamsValidator()
    {
        RuleFor(x => x)
            .NotNull()
            .NotEmpty();

        RuleForEach(x => x)
            .Must(team => team is not null && Ids.IsValid(team.Id))
            .WithMessage("Each team ID must have a valid format.");
    }
}
