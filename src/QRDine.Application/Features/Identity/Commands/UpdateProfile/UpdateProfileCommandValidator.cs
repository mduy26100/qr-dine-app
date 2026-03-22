namespace QRDine.Application.Features.Identity.Commands.UpdateProfile
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {
            RuleFor(x => x.Dto.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

            RuleFor(x => x.Dto.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

            RuleFor(x => x.Dto.PhoneNumber)
                .Must(IsValidPhone!)
                .When(x => !string.IsNullOrWhiteSpace(x.Dto.PhoneNumber))
                .WithMessage("Phone number is not valid.");
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
