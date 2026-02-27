namespace QRDine.Application.Features.Catalog.Tables.Commands.DeleteTable
{
    public class DeleteTableCommandValidator : AbstractValidator<DeleteTableCommand>
    {
        public DeleteTableCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Table ID is required for deletion.");
        }
    }
}
