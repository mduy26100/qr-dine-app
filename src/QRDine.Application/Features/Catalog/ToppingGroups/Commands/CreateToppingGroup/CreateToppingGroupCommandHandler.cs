using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Products.Specifications;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Application.Features.Catalog.ToppingGroups.DTOs;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.ToppingGroups.Commands.CreateToppingGroup
{
    public class CreateToppingGroupCommandHandler : IRequestHandler<CreateToppingGroupCommand, ToppingGroupResponseDto>
    {
        private readonly IToppingGroupRepository _toppingGroupRepository;
        private readonly IProductRepository _productRepository;

        public CreateToppingGroupCommandHandler(
            IToppingGroupRepository toppingGroupRepository,
            IProductRepository productRepository)
        {
            _toppingGroupRepository = toppingGroupRepository;
            _productRepository = productRepository;
        }

        public async Task<ToppingGroupResponseDto> Handle(CreateToppingGroupCommand request, CancellationToken cancellationToken)
        {
            var data = request.Data;

            if (data.AppliedProductIds.Any())
            {
                var spec = new ProductsCountByIdsSpec(data.AppliedProductIds);
                var validProductsCount = await _productRepository.CountAsync(spec, cancellationToken);

                if (validProductsCount != data.AppliedProductIds.Count)
                {
                    throw new BadRequestException("Một số món ăn được chọn không tồn tại hoặc không hợp lệ.");
                }
            }

            var toppingGroup = new ToppingGroup
            {
                Name = data.Name,
                Description = data.Description,
                IsRequired = data.MinSelections > 0 || data.IsRequired,
                MinSelections = data.MinSelections,
                MaxSelections = data.MaxSelections,
                IsActive = data.IsActive,

                Toppings = data.Toppings.Select(t => new Topping
                {
                    Name = t.Name,
                    Price = t.Price,
                    DisplayOrder = t.DisplayOrder,
                    IsAvailable = t.IsAvailable
                }).ToList(),

                ProductToppingGroups = data.AppliedProductIds.Select(productId => new ProductToppingGroup
                {
                    ProductId = productId
                }).ToList()
            };

            await _toppingGroupRepository.AddAsync(toppingGroup, cancellationToken);

            return new ToppingGroupResponseDto
            {
                Id = toppingGroup.Id,
                Name = toppingGroup.Name
            };
        }
    }
}
