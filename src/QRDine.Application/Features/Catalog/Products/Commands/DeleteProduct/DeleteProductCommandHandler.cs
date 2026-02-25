using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Repositories;

namespace QRDine.Application.Features.Catalog.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
    {
        private readonly IProductRepository _productRepository;

        public DeleteProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

            if (product == null || product.IsDeleted)
            {
                throw new NotFoundException($"Product with ID {request.Id} not found.");
            }

            product.IsDeleted = true;

            await _productRepository.UpdateAsync(product, cancellationToken);
        }
    }
}
