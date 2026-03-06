namespace QRDine.Application.Features.Sales.Orders.Queries.GetStorefrontOrder
{
    public class GetStorefrontOrderQueryValidator : AbstractValidator<GetStorefrontOrderQuery>
    {
        public GetStorefrontOrderQueryValidator()
        {
            RuleFor(x => x.MerchantId)
                .NotEmpty().WithMessage("MerchantId không được để trống.");

            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("SessionId không được để trống.");
        }
    }
}
