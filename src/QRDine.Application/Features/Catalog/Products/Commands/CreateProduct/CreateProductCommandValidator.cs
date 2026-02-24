namespace QRDine.Application.Features.Catalog.Products.Commands.CreateProduct
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Dto)
                .NotNull().WithMessage("Product data is required.");

            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(256).WithMessage("Product name must not exceed 256 characters.");

            RuleFor(x => x.Dto.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");

            RuleFor(x => x.Dto.CategoryId)
                .NotEmpty().WithMessage("Category ID is required.");
        }
    }
}
