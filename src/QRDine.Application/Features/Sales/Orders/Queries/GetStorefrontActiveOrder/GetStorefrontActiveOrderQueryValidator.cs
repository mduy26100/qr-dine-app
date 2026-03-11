namespace QRDine.Application.Features.Sales.Orders.Queries.GetStorefrontActiveOrder
{
    public class GetStorefrontActiveOrderQueryValidator : AbstractValidator<GetStorefrontActiveOrderQuery>
    {
        public GetStorefrontActiveOrderQueryValidator()
        {
            RuleFor(x => x.MerchantId)
                .NotEmpty().WithMessage("MerchantId không được để trống.");

            RuleFor(x => x.TableId)
                .NotEmpty().WithMessage("TableId không được để trống.");

            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("SessionId không được để trống.");
        }
    }
}
