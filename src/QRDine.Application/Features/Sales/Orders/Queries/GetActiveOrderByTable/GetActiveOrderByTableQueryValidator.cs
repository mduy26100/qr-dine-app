namespace QRDine.Application.Features.Sales.Orders.Queries.GetActiveOrderByTable
{
    public class GetActiveOrderByTableQueryValidator : AbstractValidator<GetActiveOrderByTableQuery>
    {
        public GetActiveOrderByTableQueryValidator()
        {
            RuleFor(x => x.TableId)
                .NotEmpty().WithMessage("TableId không được để trống.");
        }
    }
}
