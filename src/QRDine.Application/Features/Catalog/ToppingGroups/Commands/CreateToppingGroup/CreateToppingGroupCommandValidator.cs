namespace QRDine.Application.Features.Catalog.ToppingGroups.Commands.CreateToppingGroup
{
    public class CreateToppingGroupCommandValidator : AbstractValidator<CreateToppingGroupCommand>
    {
        public CreateToppingGroupCommandValidator()
        {
            RuleFor(x => x.Data.Name)
                .NotEmpty().WithMessage("Tên nhóm topping không được để trống.")
                .MaximumLength(100).WithMessage("Tên nhóm topping không vượt quá 100 ký tự.");

            When(x => x.Data.IsRequired, () =>
            {
                RuleFor(x => x.Data.MinSelections)
                    .GreaterThanOrEqualTo(1).WithMessage("Số lượng chọn tối thiểu phải >= 1 khi bắt buộc chọn.");
            }).Otherwise(() =>
            {
                RuleFor(x => x.Data.MinSelections)
                    .Equal(0).WithMessage("Số lượng chọn tối thiểu phải bằng 0 khi không bắt buộc chọn.");
            });

            RuleFor(x => x.Data.MaxSelections)
                .GreaterThanOrEqualTo(1).WithMessage("Số lượng chọn tối đa phải >= 1.")
                .GreaterThanOrEqualTo(x => x.Data.MinSelections).WithMessage("Số lượng tối đa phải lớn hơn hoặc bằng tối thiểu.");

            RuleFor(x => x.Data.Toppings)
                .NotEmpty().WithMessage("Nhóm topping phải có ít nhất 1 tùy chọn.");

            RuleForEach(x => x.Data.Toppings).ChildRules(topping =>
            {
                topping.RuleFor(t => t.Name)
                    .NotEmpty().WithMessage("Tên topping không được để trống.");

                topping.RuleFor(t => t.Price)
                    .GreaterThanOrEqualTo(0).WithMessage("Giá topping không được âm.");
            });
        }
    }
}