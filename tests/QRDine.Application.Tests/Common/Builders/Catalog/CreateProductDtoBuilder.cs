namespace QRDine.Application.Tests.Common.Builders.Catalog
{
    public class CreateProductDtoBuilder
    {
        private string _name = "Laptop";
        private string? _description = "High-end laptop";
        private decimal _price = 1000m;
        private bool _isAvailable = true;
        private Guid _categoryId = Guid.NewGuid();
        private Stream? _imgContent = null;
        private string? _imgFileName = null;
        private string? _imgContentType = null;

        public CreateProductDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public CreateProductDtoBuilder WithDescription(string? description)
        {
            _description = description;
            return this;
        }

        public CreateProductDtoBuilder WithPrice(decimal price)
        {
            _price = price;
            return this;
        }

        public CreateProductDtoBuilder WithIsAvailable(bool isAvailable)
        {
            _isAvailable = isAvailable;
            return this;
        }

        public CreateProductDtoBuilder WithCategoryId(Guid categoryId)
        {
            _categoryId = categoryId;
            return this;
        }

        public CreateProductDtoBuilder WithImage(Stream? content, string? fileName = null, string? contentType = null)
        {
            _imgContent = content;
            _imgFileName = fileName;
            _imgContentType = contentType;
            return this;
        }

        public CreateProductDto Build()
        {
            return new CreateProductDto
            {
                Name = _name,
                Description = _description,
                Price = _price,
                IsAvailable = _isAvailable,
                CategoryId = _categoryId,
                ImgContent = _imgContent,
                ImgFileName = _imgFileName,
                ImgContentType = _imgContentType
            };
        }
    }
}
