namespace QRDine.Application.Features.Tenant.Merchants.Commands.UpdateMerchant
{
    public class UpdateMerchantCommandValidator : AbstractValidator<UpdateMerchantCommand>
    {
        public UpdateMerchantCommandValidator()
        {
            RuleFor(x => x.Dto)
                .NotNull().WithMessage("Merchant data is required.");

            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage("Merchant name is required.")
                .MaximumLength(256).WithMessage("Merchant name must not exceed 256 characters.");

            RuleFor(x => x.Dto.Slug)
                .NotEmpty().WithMessage("Merchant slug is required.")
                .MaximumLength(256).WithMessage("Merchant slug must not exceed 256 characters.");

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
