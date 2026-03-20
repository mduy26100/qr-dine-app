namespace QRDine.Application.Features.Reports.Queries.GetProductPerformance
{
    public class GetProductPerformanceQueryValidator : AbstractValidator<GetProductPerformanceQuery>
    {
        public GetProductPerformanceQueryValidator()
        {
            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Ngày bắt đầu không được để trống.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("Ngày kết thúc không được để trống.")
                .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");

            RuleFor(x => x.SortBy)
                .IsInEnum().WithMessage("Kiểu sắp xếp không hợp lệ. Chỉ chấp nhận: Revenue (1), Volume (2).");

            RuleFor(x => x.Top)
                .GreaterThan(0).WithMessage("Top phải lớn hơn 0.")
                .LessThanOrEqualTo(10000).WithMessage("Top không được vượt quá 10000.");
        }
    }
}
