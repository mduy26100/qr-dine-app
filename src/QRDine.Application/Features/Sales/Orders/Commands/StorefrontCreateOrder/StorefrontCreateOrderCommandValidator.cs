using QRDine.Application.Features.Sales.Orders.DTOs;

namespace QRDine.Application.Features.Sales.Orders.Commands.StorefrontCreateOrder
{
    public class StorefrontCreateOrderCommandValidator : AbstractValidator<StorefrontCreateOrderCommand>
    {
        public StorefrontCreateOrderCommandValidator()
        {
            RuleFor(x => x.MerchantId)
                .NotEmpty().WithMessage("MerchantId không được để trống.");

            RuleFor(x => x.Dto)
                .NotNull().WithMessage("Payload không được để trống.");

            When(x => x.Dto != null, () =>
            {
                RuleFor(x => x.Dto.TableId)
                    .NotEmpty().WithMessage("TableId không được để trống.");

                RuleFor(x => x.Dto.SessionId)
                    .NotEmpty().WithMessage("SessionId không được để trống.");

                RuleFor(x => x.Dto.CustomerName)
                    .MaximumLength(256).WithMessage("Tên khách hàng không được vượt quá 256 ký tự.");

                RuleFor(x => x.Dto.CustomerPhone)
                    .MaximumLength(20).WithMessage("Số điện thoại không được vượt quá 20 ký tự.")
                    .Matches(@"^0\d{9,10}$").When(x => !string.IsNullOrEmpty(x.Dto.CustomerPhone))
                    .WithMessage("Số điện thoại không hợp lệ.");

                RuleFor(x => x.Dto.Note)
                    .MaximumLength(1000).WithMessage("Ghi chú đơn hàng không được vượt quá 1000 ký tự.");

                RuleFor(x => x.Dto.Items)
                    .NotEmpty().WithMessage("Đơn hàng phải có ít nhất 1 món.")
                    .Must(items => items != null && items.Count > 0).WithMessage("Danh sách món ăn không hợp lệ.");

                RuleForEach(x => x.Dto.Items).SetValidator(new StorefrontCreateOrderItemDtoValidator());
            });
        }
    }

    public class StorefrontCreateOrderItemDtoValidator : AbstractValidator<StorefrontCreateOrderItemDto>
    {
        public StorefrontCreateOrderItemDtoValidator()
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
