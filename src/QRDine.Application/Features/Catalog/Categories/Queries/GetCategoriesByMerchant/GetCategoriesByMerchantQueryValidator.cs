using QRDine.Application.Common.Abstractions.Identity;

namespace QRDine.Application.Features.Catalog.Categories.Queries.GetCategoriesByMerchant
{
    public class GetCategoriesByMerchantQueryValidator : AbstractValidator<GetCategoriesByMerchantQuery>
    {
        public GetCategoriesByMerchantQueryValidator(ICurrentUserService currentUserService)
        {
            RuleFor(x => x.MerchantId)
                .Must(merchantId => merchantId.HasValue || currentUserService.MerchantId.HasValue)
                .WithMessage("MerchantId is required. Please provide it in the request or authenticate as a merchant.");
        }
    }
}
