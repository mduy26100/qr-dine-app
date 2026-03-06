namespace QRDine.Application.Features.Catalog.Categories.Queries.GetStorefrontMenu
{
    public class GetStorefrontMenuQueryValidator : AbstractValidator<GetStorefrontMenuQuery>
    {
        public GetStorefrontMenuQueryValidator()
        {
            RuleFor(x => x.MerchantId)
                .NotEmpty().WithMessage("MerchantId không được để trống.");
        }
    }
}
