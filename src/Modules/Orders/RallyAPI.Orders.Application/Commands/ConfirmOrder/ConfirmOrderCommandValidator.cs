using FluentValidation;

namespace RallyAPI.Orders.Application.Commands.ConfirmOrder;

/// <summary>
/// Validator for ConfirmOrderCommand.
/// </summary>
public sealed class ConfirmOrderCommandValidator : AbstractValidator<ConfirmOrderCommand>
{
    public ConfirmOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.RestaurantId)
            .NotEmpty()
            .WithMessage("Restaurant ID is required");
    }
}
