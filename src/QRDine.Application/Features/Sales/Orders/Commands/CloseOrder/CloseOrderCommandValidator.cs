using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Sales.Orders.Commands.CloseOrder
{
    public class CloseOrderCommandValidator : AbstractValidator<CloseOrderCommand>
    {
        public CloseOrderCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("OrderId không được để trống.");

            RuleFor(x => x.TargetStatus)
                .Must(status => status == OrderStatus.Paid || status == OrderStatus.Cancelled)
                .WithMessage("Trạng thái không hợp lệ. Chỉ chấp nhận Paid hoặc Cancelled.");
        }
    }
}
