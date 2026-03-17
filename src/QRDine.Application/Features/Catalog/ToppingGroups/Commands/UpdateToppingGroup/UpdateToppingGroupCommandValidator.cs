namespace QRDine.Application.Features.Catalog.ToppingGroups.Commands.UpdateToppingGroup
{
    public class UpdateToppingGroupCommandValidator : AbstractValidator<UpdateToppingGroupCommand>
    {
        public UpdateToppingGroupCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID nhóm topping không được để trống.");

            RuleFor(x => x.Data.Name)
                .NotEmpty().WithMessage("Tên nhóm topping không được để trống.")
                .MaximumLength(100).WithMessage("Tên nhóm topping không vượt quá 100 ký tự.");

            RuleFor(x => x.Data.MinSelections)
                .GreaterThanOrEqualTo(0).WithMessage("Số lượng chọn tối thiểu phải >= 0.");

            RuleFor(x => x.Data.MaxSelections)
                .GreaterThanOrEqualTo(1).WithMessage("Số lượng chọn tối đa phải >= 1.")
                .GreaterThanOrEqualTo(x => x.Data.MinSelections).WithMessage("Số lượng tối đa phải lớn hơn hoặc bằng tối thiểu.");

            RuleFor(x => x.Data.Toppings)
                .NotEmpty().WithMessage("Nhóm topping phải có ít nhất 1 tùy chọn.");

            RuleForEach(x => x.Data.Toppings).ChildRules(topping =>
            {
                topping.RuleFor(t => t.Id)
                    .NotEqual(Guid.Empty).WithMessage("ID topping không hợp lệ.")
                    .When(t => t.Id.HasValue);

                topping.RuleFor(t => t.Name)
                    .NotEmpty().WithMessage("Tên topping không được để trống.");

                topping.RuleFor(t => t.Price)
                    .GreaterThanOrEqualTo(0).WithMessage("Giá topping không được âm.");
            });
        }
    }
}
