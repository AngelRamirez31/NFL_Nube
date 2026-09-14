using FluentValidation;
using TournamentServices.Api.Common;
using TournamentServices.Api.Dtos;

namespace TournamentServices.Api.Validators;

public class CreateGroupDtoValidator
    : AbstractValidator<CreateGroupDto>
{
    public CreateGroupDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}

public class UpdateGroupDtoValidator
    : AbstractValidator<UpdateGroupDto>
{
    public UpdateGroupDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}

public class AssignTeamsDtoValidator
    : AbstractValidator<AssignTeamsDto>
{
    public AssignTeamsDtoValidator()
    {
        RuleFor(x => x.TeamIds)
            .NotNull()
            .NotEmpty();

        RuleForEach(x => x.TeamIds)
            .NotEmpty()
            .Must(Ids.IsValid)
            .WithMessage("Each team ID must have a valid format.");
    }
}