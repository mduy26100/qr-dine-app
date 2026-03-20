namespace QRDine.Application.Features.Reports.Queries.GetRevenueChart
{
    public class GetRevenueChartQueryValidator : AbstractValidator<GetRevenueChartQuery>
    {
        public GetRevenueChartQueryValidator()
        {
            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Ngày bắt đầu không được để trống.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("Ngày kết thúc không được để trống.")
                .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");

            RuleFor(x => x.Grouping)
                .IsInEnum().WithMessage("Kiểu nhóm không hợp lệ. Chỉ chấp nhận: ByHour (1), ByDay (2), ByMonth (3).");
        }
    }
}
