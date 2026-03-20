namespace QRDine.Application.Features.Reports.Queries.GetTrafficHeatmap
{
    public class GetTrafficHeatmapQueryValidator : AbstractValidator<GetTrafficHeatmapQuery>
    {
        public GetTrafficHeatmapQueryValidator()
        {
            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Ngày bắt đầu không được để trống.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("Ngày kết thúc không được để trống.")
                .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
        }
    }
}
