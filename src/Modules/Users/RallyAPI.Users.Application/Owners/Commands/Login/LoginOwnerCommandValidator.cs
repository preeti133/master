using FluentValidation;

namespace RallyAPI.Users.Application.Owners.Commands.Login;

public sealed class LoginOwnerCommandValidator : AbstractValidator<LoginOwnerCommand>
{
    public LoginOwnerCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
