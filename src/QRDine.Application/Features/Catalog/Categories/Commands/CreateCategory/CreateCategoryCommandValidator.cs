namespace QRDine.Application.Features.Catalog.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(x => x.Dto)
                .NotNull().WithMessage("Category data must be provided.");

            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

            RuleFor(x => x.Dto.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
        }
    }
}
