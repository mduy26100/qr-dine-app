using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Products.Specifications;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Application.Features.Catalog.ToppingGroups.Specifications;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.ToppingGroups.Commands.UpdateToppingGroup
{
    public class UpdateToppingGroupCommandHandler : IRequestHandler<UpdateToppingGroupCommand, Unit>
    {
        private readonly IToppingGroupRepository _toppingGroupRepository;
        private readonly IProductRepository _productRepository;

        public UpdateToppingGroupCommandHandler(
            IToppingGroupRepository toppingGroupRepository,
            IProductRepository productRepository)
        {
            _toppingGroupRepository = toppingGroupRepository;
            _productRepository = productRepository;
        }

        public async Task<Unit> Handle(UpdateToppingGroupCommand request, CancellationToken cancellationToken)
        {
            var data = request.Data;

            var spec = new ToppingGroupWithDetailsByIdSpec(request.Id);
            var toppingGroup = await _toppingGroupRepository.SingleOrDefaultAsync(spec, cancellationToken);

            if (toppingGroup == null)
            {
                throw new NotFoundException("Không tìm thấy nhóm Topping này.");
            }

            if (data.AppliedProductIds.Any())
            {
                var productSpec = new ProductsCountByIdsSpec(data.AppliedProductIds);
                var validCount = await _productRepository.CountAsync(productSpec, cancellationToken);
                if (validCount != data.AppliedProductIds.Count)
                {
                    throw new BadRequestException("Một số món ăn được gán không tồn tại hoặc đã bị xóa.");
                }
            }

            toppingGroup.Name = data.Name;
            toppingGroup.Description = data.Description;
            toppingGroup.IsRequired = data.MinSelections > 0 || data.IsRequired;
            toppingGroup.MinSelections = data.MinSelections;
            toppingGroup.MaxSelections = data.MaxSelections;
            toppingGroup.IsActive = data.IsActive;

            var incomingToppingIds = data.Toppings.Where(t => t.Id.HasValue).Select(t => t.Id!.Value).ToList();

            var activeToppings = toppingGroup.Toppings.Where(t => !t.IsDeleted).ToList();
            var toppingsToRemove = activeToppings.Where(t => !incomingToppingIds.Contains(t.Id)).ToList();
            foreach (var t in toppingsToRemove)
            {
                t.IsDeleted = true;
            }

            foreach (var tDto in data.Toppings)
            {
                if (tDto.Id.HasValue)
                {
                    var existingTopping = toppingGroup.Toppings.FirstOrDefault(t => t.Id == tDto.Id.Value);
                    if (existingTopping != null)
                    {
                        existingTopping.Name = tDto.Name;
                        existingTopping.Price = tDto.Price;
                        existingTopping.DisplayOrder = tDto.DisplayOrder;
                        existingTopping.IsAvailable = tDto.IsAvailable;
                        existingTopping.IsDeleted = false;
                    }
                }
                else
                {
                    var resurrectedTopping = toppingGroup.Toppings
                        .FirstOrDefault(t => t.IsDeleted &&
                                             t.Name.Trim().Equals(tDto.Name.Trim(), StringComparison.OrdinalIgnoreCase));

                    if (resurrectedTopping != null)
                    {
                        resurrectedTopping.Name = tDto.Name;
                        resurrectedTopping.Price = tDto.Price;
                        resurrectedTopping.DisplayOrder = tDto.DisplayOrder;
                        resurrectedTopping.IsAvailable = tDto.IsAvailable;
                        resurrectedTopping.IsDeleted = false;
                    }
                    else
                    {
                        toppingGroup.Toppings.Add(new Topping
                        {
                            Name = tDto.Name,
                            Price = tDto.Price,
                            DisplayOrder = tDto.DisplayOrder,
                            IsAvailable = tDto.IsAvailable
                        });
                    }
                }
            }

            var currentProductIds = toppingGroup.ProductToppingGroups.Select(p => p.ProductId).ToList();

            var mappingsToRemove = toppingGroup.ProductToppingGroups.Where(p => !data.AppliedProductIds.Contains(p.ProductId)).ToList();
            foreach (var m in mappingsToRemove)
            {
                toppingGroup.ProductToppingGroups.Remove(m);
            }

            var newProductIds = data.AppliedProductIds.Where(id => !currentProductIds.Contains(id)).ToList();
            foreach (var newId in newProductIds)
            {
                toppingGroup.ProductToppingGroups.Add(new ProductToppingGroup { ProductId = newId });
            }

            await _toppingGroupRepository.UpdateAsync(toppingGroup, cancellationToken);

            return Unit.Value;
        }
    }
}
