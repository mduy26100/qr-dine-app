using QRDine.Application.Features.Catalog.Products.DTOs;

namespace QRDine.API.Requests.Catalog
{
    public class CreateProductForm
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;

        public Guid CategoryId { get; set; }

        public IFormFile? ImageFile { get; set; }

        public CreateProductDto ToDto()
        {
            return new CreateProductDto
            {
                Name = Name,
                Description = Description,
                Price = Price,
                IsAvailable = IsAvailable,
                CategoryId = CategoryId,
                ImgContent = ImageFile?.OpenReadStream(),
                ImgFileName = ImageFile?.FileName,
                ImgContentType = ImageFile?.ContentType
            };
        }
    }
}
