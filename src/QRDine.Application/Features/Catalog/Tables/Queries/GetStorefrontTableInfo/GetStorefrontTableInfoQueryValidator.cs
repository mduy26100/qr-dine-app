namespace QRDine.Application.Features.Catalog.Tables.Queries.GetStorefrontTableInfo
{
    public class GetStorefrontTableInfoQueryValidator : AbstractValidator<GetStorefrontTableInfoQuery>
    {
        public GetStorefrontTableInfoQueryValidator()
        {
            RuleFor(x => x.MerchantId)
                .NotEmpty().WithMessage("MerchantId không được để trống.");

            RuleFor(x => x.QrCodeToken)
                .NotEmpty().WithMessage("QrCodeToken không hợp lệ.");
        }
    }
}
