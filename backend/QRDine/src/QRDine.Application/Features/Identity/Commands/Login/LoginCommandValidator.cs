namespace QRDine.Application.Features.Identity.Commands.Login
{
    internal class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Dto.Identifier)
                .NotEmpty().WithMessage("Email or phone number is required.")
                .Must(BeValidEmailOrPhone)
                .WithMessage("Identifier must be a valid email address or phone number.");

            RuleFor(x => x.Dto.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        }

        private bool BeValidEmailOrPhone(string identifier)
        {
            return IsValidEmail(identifier) || IsValidPhone(identifier);
        }

        private bool IsValidEmail(string email)
        {
            var emailAttribute = new System.ComponentModel.DataAnnotations.EmailAddressAttribute();
            return emailAttribute.IsValid(email);
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            phone = phone.Trim();

            return System.Text.RegularExpressions.Regex.IsMatch(
                phone,
                @"^\+?[0-9]{9,15}$"
            );
        }
    }
}
