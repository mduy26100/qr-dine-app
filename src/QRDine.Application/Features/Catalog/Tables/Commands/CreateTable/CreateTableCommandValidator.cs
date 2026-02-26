namespace QRDine.Application.Features.Catalog.Tables.Commands.CreateTable
{
    public class CreateTableCommandValidator : AbstractValidator<CreateTableCommand>
    {
        public CreateTableCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Table name is required.")
                .MaximumLength(100).WithMessage("Table name must not exceed 100 characters.");
        }
    }
}
