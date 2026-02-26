namespace QRDine.Application.Features.Catalog.Products.Queries.GetProductsByCategory
{
    public class GetProductsByCategoryQueryValidator : AbstractValidator<GetProductsByCategoryQuery>
    {
        public GetProductsByCategoryQueryValidator()
        {
            RuleFor(x => x.MerchantId)
                .NotEmpty()
                .WithMessage("MerchantId is required for storefront catalog.");

            RuleFor(x => x.CategoryId)
                .NotEmpty()
                .WithMessage("CategoryId is required to fetch products.");
        }
    }
}
