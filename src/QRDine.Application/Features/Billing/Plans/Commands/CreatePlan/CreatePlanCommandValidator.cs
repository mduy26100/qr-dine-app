namespace QRDine.Application.Features.Billing.Plans.Commands.CreatePlan
{
    public class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
    {
        public CreatePlanCommandValidator()
        {
            RuleFor(x => x.Dto.Code)
                .NotEmpty().WithMessage("Mã gói không được để trống.")
                .MaximumLength(50).WithMessage("Mã gói không vượt quá 50 ký tự.");

            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage("Tên gói không được để trống.")
                .MaximumLength(150).WithMessage("Tên gói không vượt quá 150 ký tự.");

            RuleFor(x => x.Dto.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Giá tiền phải lớn hơn hoặc bằng 0.");

            RuleFor(x => x.Dto.DurationDays)
                .GreaterThan(0).WithMessage("Thời hạn của gói phải lớn hơn 0 ngày.");
        }
    }
}
