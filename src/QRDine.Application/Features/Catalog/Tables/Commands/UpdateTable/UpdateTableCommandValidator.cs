namespace QRDine.Application.Features.Catalog.Tables.Commands.UpdateTable
{
    public class UpdateTableCommandValidator : AbstractValidator<UpdateTableCommand>
    {
        public UpdateTableCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Table ID is required.");

            RuleFor(x => x.Dto)
                .NotNull().WithMessage("Table data is required.");

            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage("Table name is required.")
                .MaximumLength(100).WithMessage("Table name must not exceed 100 characters.");
        }
    }
}
