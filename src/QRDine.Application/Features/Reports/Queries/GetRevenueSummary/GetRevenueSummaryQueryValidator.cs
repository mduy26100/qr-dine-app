namespace QRDine.Application.Features.Reports.Queries.GetRevenueSummary
{
    public class GetRevenueSummaryQueryValidator : AbstractValidator<GetRevenueSummaryQuery>
    {
        public GetRevenueSummaryQueryValidator()
        {
            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Ngày bắt đầu không được để trống.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("Ngày kết thúc không được để trống.")
                .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
        }
    }
}
