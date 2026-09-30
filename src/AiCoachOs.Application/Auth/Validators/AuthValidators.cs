using AiCoachOs.Application.Auth.DTOs;
using FluentValidation;

namespace AiCoachOs.Application.Auth.Validators;

public class RegisterCoachRequestValidator : AbstractValidator<RegisterCoachRequestDto>
{
    public RegisterCoachRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        // Mirrors the Identity password policy configured in
        // AiCoachOs.Infrastructure/DependencyInjection.cs. Keep the two in step so a rejected
        // password fails validation with a useful message instead of surfacing as an Identity error.
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a non-alphanumeric character.");
    }
}

public class LoginCoachRequestValidator : AbstractValidator<LoginCoachRequestDto>
{
    public LoginCoachRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
