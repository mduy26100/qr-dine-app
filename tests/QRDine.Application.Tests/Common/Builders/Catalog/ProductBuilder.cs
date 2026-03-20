namespace QRDine.Application.Tests.Common.Builders.Catalog
{
    public class ProductBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid _merchantId = Guid.NewGuid();
        private Guid _categoryId = Guid.NewGuid();
        private string _name = "Laptop";
        private string? _description = "High-end laptop";
        private string? _imageUrl = null;
        private decimal _price = 1000m;
        private bool _isAvailable = true;
        private bool _isDeleted = false;

        public ProductBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public ProductBuilder WithMerchantId(Guid merchantId)
        {
            _merchantId = merchantId;
            return this;
        }

        public ProductBuilder WithCategoryId(Guid categoryId)
        {
            _categoryId = categoryId;
            return this;
        }

        public ProductBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ProductBuilder WithDescription(string? description)
        {
            _description = description;
            return this;
        }

        public ProductBuilder WithImageUrl(string? imageUrl)
        {
            _imageUrl = imageUrl;
            return this;
        }

        public ProductBuilder WithPrice(decimal price)
        {
            _price = price;
            return this;
        }

        public ProductBuilder WithIsAvailable(bool isAvailable)
        {
            _isAvailable = isAvailable;
            return this;
        }

        public ProductBuilder WithIsDeleted(bool isDeleted)
        {
            _isDeleted = isDeleted;
            return this;
        }

        public Product Build()
        {
            return new Product
            {
                Id = _id,
                MerchantId = _merchantId,
                CategoryId = _categoryId,
                Name = _name,
                Description = _description,
                ImageUrl = _imageUrl,
                Price = _price,
                IsAvailable = _isAvailable,
                IsDeleted = _isDeleted
            };
        }
    }
}
