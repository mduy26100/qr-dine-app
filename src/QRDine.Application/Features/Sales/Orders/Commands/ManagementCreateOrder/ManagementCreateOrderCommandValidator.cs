using QRDine.Application.Features.Sales.Orders.DTOs;

namespace QRDine.Application.Features.Sales.Orders.Commands.ManagementCreateOrder
{
    public class ManagementCreateOrderCommandValidator : AbstractValidator<ManagementCreateOrderCommand>
    {
        public ManagementCreateOrderCommandValidator()
        {
            RuleFor(x => x.Dto)
                .NotNull().WithMessage("Payload không được để trống.");

            When(x => x.Dto != null, () =>
            {
                RuleFor(x => x.Dto.TableId)
                    .NotEmpty().WithMessage("TableId không được để trống.");

                RuleFor(x => x.Dto.Note)
                    .MaximumLength(1000).WithMessage("Ghi chú đơn hàng không được vượt quá 1000 ký tự.");

                RuleFor(x => x.Dto.Items)
                    .NotEmpty().WithMessage("Đơn hàng phải có ít nhất 1 món.")
                    .Must(items => items != null && items.Count > 0).WithMessage("Danh sách món ăn không hợp lệ.");

                RuleForEach(x => x.Dto.Items).SetValidator(new ManagementCreateOrderItemDtoValidator());
            });
        }
    }

    public class ManagementCreateOrderItemDtoValidator : AbstractValidator<ManagementCreateOrderItemDto>
    {
        public ManagementCreateOrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("ProductId không được để trống.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Số lượng món ăn phải lớn hơn 0.");

            RuleFor(x => x.ToppingSurcharge)
                .GreaterThanOrEqualTo(0).WithMessage("Phụ phí Topping không được là số âm.");

            RuleFor(x => x.ToppingsSnapshot)
                .MaximumLength(1000).WithMessage("Chuỗi Topping không được vượt quá 1000 ký tự.");

            RuleFor(x => x.Note)
                .MaximumLength(500).WithMessage("Ghi chú món ăn không được vượt quá 500 ký tự.");
        }
    }
}
