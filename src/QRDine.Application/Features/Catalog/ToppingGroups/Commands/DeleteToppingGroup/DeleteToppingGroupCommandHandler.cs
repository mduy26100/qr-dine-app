using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Application.Features.Catalog.ToppingGroups.Specifications;

namespace QRDine.Application.Features.Catalog.ToppingGroups.Commands.DeleteToppingGroup
{
    public class DeleteToppingGroupCommandHandler : IRequestHandler<DeleteToppingGroupCommand, Unit>
    {
        private readonly IToppingGroupRepository _toppingGroupRepository;

        public DeleteToppingGroupCommandHandler(IToppingGroupRepository toppingGroupRepository)
        {
            _toppingGroupRepository = toppingGroupRepository;
        }

        public async Task<Unit> Handle(DeleteToppingGroupCommand request, CancellationToken cancellationToken)
        {
            var spec = new ToppingGroupWithDetailsByIdSpec(request.Id);
            var toppingGroup = await _toppingGroupRepository.SingleOrDefaultAsync(spec, cancellationToken);

            if (toppingGroup == null)
            {
                throw new NotFoundException("Không tìm thấy nhóm Topping này hoặc đã bị xóa.");
            }

            toppingGroup.IsDeleted = true;

            foreach (var topping in toppingGroup.Toppings)
            {
                topping.IsDeleted = true;
            }

            toppingGroup.ProductToppingGroups.Clear();

            await _toppingGroupRepository.UpdateAsync(toppingGroup, cancellationToken);

            return Unit.Value;
        }
    }
}
