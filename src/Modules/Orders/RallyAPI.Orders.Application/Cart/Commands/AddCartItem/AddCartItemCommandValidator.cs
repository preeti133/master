using FluentValidation;

namespace RallyAPI.Orders.Application.Cart.Commands.AddCartItem;

public sealed class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.RestaurantName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MenuItemId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(50);
        RuleFor(x => x.SpecialInstructions).MaximumLength(500).When(x => x.SpecialInstructions != null);
    }
}
