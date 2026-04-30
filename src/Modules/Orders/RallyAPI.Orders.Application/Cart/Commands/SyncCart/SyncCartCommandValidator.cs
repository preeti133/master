using FluentValidation;

namespace RallyAPI.Orders.Application.Cart.Commands.SyncCart;

public sealed class SyncCartCommandValidator : AbstractValidator<SyncCartCommand>
{
    public SyncCartCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.RestaurantName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Items).NotNull();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.MenuItemId).NotEmpty();
            item.RuleFor(i => i.Name).NotEmpty().MaximumLength(200);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0).LessThanOrEqualTo(50);
        });
    }
}
