namespace QRDine.Application.Features.Sales.OrderItems.Commands.UpdateOrderItemsStatus
{
    public class UpdateOrderItemsStatusCommandValidator : AbstractValidator<UpdateOrderItemsStatusCommand>
    {
        public UpdateOrderItemsStatusCommandValidator()
        {
            RuleFor(x => x.OrderItemIds)
                .NotNull().WithMessage("Danh sách ID món ăn không được để trống.")
                .NotEmpty().WithMessage("Phải chọn ít nhất một món ăn để cập nhật.");

            RuleForEach(x => x.OrderItemIds)
                .NotEmpty().WithMessage("ID món ăn không hợp lệ.");

            RuleFor(x => x.TargetStatus)
                .IsInEnum().WithMessage("Trạng thái đích không hợp lệ. Chỉ chấp nhận các giá trị: Pending (1), Preparing (2), Served (3), Cancelled (4).");
        }
    }
}
