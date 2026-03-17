namespace QRDine.Application.Features.Catalog.ToppingGroups.Commands.DeleteToppingGroup
{
    public class DeleteToppingGroupCommandValidator : AbstractValidator<DeleteToppingGroupCommand>
    {
        public DeleteToppingGroupCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID nhóm topping không được để trống.");
        }
    }
}
