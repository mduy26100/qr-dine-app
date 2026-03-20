namespace QRDine.Application.Tests.Common.Builders.Catalog
{
    public class UpdateProductDtoBuilder
    {
        private string _name = "Laptop Updated";
        private string? _description = "Updated high-end laptop";
        private decimal _price = 1200m;
        private bool _isAvailable = true;
        private Guid _categoryId = Guid.NewGuid();
        private Stream? _imgContent = null;
        private string? _imgFileName = null;
        private string? _imgContentType = null;

        public UpdateProductDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public UpdateProductDtoBuilder WithDescription(string? description)
        {
            _description = description;
            return this;
        }

        public UpdateProductDtoBuilder WithPrice(decimal price)
        {
            _price = price;
            return this;
        }

        public UpdateProductDtoBuilder WithIsAvailable(bool isAvailable)
        {
            _isAvailable = isAvailable;
            return this;
        }

        public UpdateProductDtoBuilder WithCategoryId(Guid categoryId)
        {
            _categoryId = categoryId;
            return this;
        }

        public UpdateProductDtoBuilder WithImage(Stream? content, string? fileName = null, string? contentType = null)
        {
            _imgContent = content;
            _imgFileName = fileName;
            _imgContentType = contentType;
            return this;
        }

        public UpdateProductDto Build()
        {
            return new UpdateProductDto
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
