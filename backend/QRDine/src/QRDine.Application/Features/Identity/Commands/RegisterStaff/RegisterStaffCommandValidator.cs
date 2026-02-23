namespace QRDine.Application.Features.Identity.Commands.RegisterStaff
{
    public class RegisterStaffCommandValidator : AbstractValidator<RegisterStaffCommand>
    {
        public RegisterStaffCommandValidator()
        {
            RuleFor(x => x.Dto.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

            RuleFor(x => x.Dto.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

            RuleFor(x => x.Dto.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Dto.PhoneNumber)
                .Must(IsValidPhone!)
                .When(x => !string.IsNullOrWhiteSpace(x.Dto.PhoneNumber))
                .WithMessage("User phone number is not valid.");

            RuleFor(x => x.Dto.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Password is required.")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).+$")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;
            phone = phone.Trim();
            return Regex.IsMatch(phone, @"^\+?[0-9]{9,15}$");
        }
    }
}
