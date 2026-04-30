using FluentValidation;

namespace RallyAPI.Users.Application.Owners.Commands.SwitchOutlet;

public sealed class SwitchToOutletCommandValidator : AbstractValidator<SwitchToOutletCommand>
{
    public SwitchToOutletCommandValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.RestaurantId).NotEmpty();
    }
}
