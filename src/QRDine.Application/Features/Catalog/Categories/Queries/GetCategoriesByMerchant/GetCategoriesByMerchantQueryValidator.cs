namespace QRDine.Application.Features.Catalog.Categories.Queries.GetCategoriesByMerchant
{
    public class GetCategoriesByMerchantQueryValidator : AbstractValidator<GetCategoriesByMerchantQuery>
    {
        public GetCategoriesByMerchantQueryValidator()
        {
            RuleFor(x => x.MerchantId)
                .NotEmpty()
                .WithMessage("MerchantId is required for storefront catalog.");
        }
    }
}
