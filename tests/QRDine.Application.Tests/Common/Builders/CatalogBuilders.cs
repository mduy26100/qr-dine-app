namespace QRDine.Application.Tests.Common.Builders
{
    public class CreateCategoryDtoBuilder
    {
        private string _name = "Electronics";
        private string? _description = "Electronic products";
        private int _displayOrder = 1;
        private bool _isActive = true;
        private Guid? _parentId = null;

        public CreateCategoryDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public CreateCategoryDtoBuilder WithDescription(string? description)
        {
            _description = description;
            return this;
        }

        public CreateCategoryDtoBuilder WithDisplayOrder(int displayOrder)
        {
            _displayOrder = displayOrder;
            return this;
        }

        public CreateCategoryDtoBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public CreateCategoryDtoBuilder WithParentId(Guid? parentId)
        {
            _parentId = parentId;
            return this;
        }

        public CreateCategoryDto Build()
        {
            return new CreateCategoryDto
            {
                Name = _name,
                Description = _description,
                DisplayOrder = _displayOrder,
                IsActive = _isActive,
                ParentId = _parentId
            };
        }
    }

    public class UpdateCategoryDtoBuilder
    {
        private string _name = "Electronics Updated";
        private string? _description = "Updated electronic products";
        private int _displayOrder = 1;
        private bool _isActive = true;
        private Guid? _parentId = null;

        public UpdateCategoryDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public UpdateCategoryDtoBuilder WithDescription(string? description)
        {
            _description = description;
            return this;
        }

        public UpdateCategoryDtoBuilder WithDisplayOrder(int displayOrder)
        {
            _displayOrder = displayOrder;
            return this;
        }

        public UpdateCategoryDtoBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public UpdateCategoryDtoBuilder WithParentId(Guid? parentId)
        {
            _parentId = parentId;
            return this;
        }

        public UpdateCategoryDto Build()
        {
            return new UpdateCategoryDto
            {
                Name = _name,
                Description = _description,
                DisplayOrder = _displayOrder,
                IsActive = _isActive,
                ParentId = _parentId
            };
        }
    }

    public class CategoryBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid _merchantId = Guid.NewGuid();
        private string _name = "Electronics";
        private string? _description = "Electronic products";
        private int _displayOrder = 1;
        private bool _isActive = true;
        private Guid? _parentId = null;
        private bool _isDeleted = false;

        public CategoryBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public CategoryBuilder WithMerchantId(Guid merchantId)
        {
            _merchantId = merchantId;
            return this;
        }

        public CategoryBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public CategoryBuilder WithDescription(string? description)
        {
            _description = description;
            return this;
        }

        public CategoryBuilder WithDisplayOrder(int displayOrder)
        {
            _displayOrder = displayOrder;
            return this;
        }

        public CategoryBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public CategoryBuilder WithParentId(Guid? parentId)
        {
            _parentId = parentId;
            return this;
        }

        public CategoryBuilder WithIsDeleted(bool isDeleted)
        {
            _isDeleted = isDeleted;
            return this;
        }

        public Category Build()
        {
            return new Category
            {
                Id = _id,
                MerchantId = _merchantId,
                Name = _name,
                Description = _description,
                DisplayOrder = _displayOrder,
                IsActive = _isActive,
                ParentId = _parentId,
                IsDeleted = _isDeleted
            };
        }
    }

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

    public class UpdateTableDtoBuilder
    {
        private string _name = "Table 1 Updated";

        public UpdateTableDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public UpdateTableDto Build()
        {
            return new UpdateTableDto
            {
                Name = _name
            };
        }
    }

    public class TableBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid _merchantId = Guid.NewGuid();
        private string _name = "Table 1";
        private bool _isOccupied = false;
        private string? _qrCodeToken = Guid.NewGuid().ToString("N");
        private string? _qrCodeImageUrl = "https://example.com/qr.png";
        private Guid? _currentSessionId = null;
        private bool _isDeleted = false;

        public TableBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public TableBuilder WithMerchantId(Guid merchantId)
        {
            _merchantId = merchantId;
            return this;
        }

        public TableBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public TableBuilder WithIsOccupied(bool isOccupied)
        {
            _isOccupied = isOccupied;
            return this;
        }

        public TableBuilder WithQrCodeToken(string? token)
        {
            _qrCodeToken = token;
            return this;
        }

        public TableBuilder WithQrCodeImageUrl(string? url)
        {
            _qrCodeImageUrl = url;
            return this;
        }

        public TableBuilder WithCurrentSessionId(Guid? sessionId)
        {
            _currentSessionId = sessionId;
            return this;
        }

        public TableBuilder WithIsDeleted(bool isDeleted)
        {
            _isDeleted = isDeleted;
            return this;
        }

        public Table Build()
        {
            return new Table
            {
                Id = _id,
                MerchantId = _merchantId,
                Name = _name,
                IsOccupied = _isOccupied,
                QrCodeToken = _qrCodeToken,
                QrCodeImageUrl = _qrCodeImageUrl,
                CurrentSessionId = _currentSessionId,
                IsDeleted = _isDeleted,
                RowVersion = new byte[] { }
            };
        }
    }

    public class ToppingRequestDtoBuilder
    {
        private string _name = "Extra Cheese";
        private decimal _price = 50m;
        private int _displayOrder = 1;
        private bool _isAvailable = true;

        public ToppingRequestDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ToppingRequestDtoBuilder WithPrice(decimal price)
        {
            _price = price;
            return this;
        }

        public ToppingRequestDtoBuilder WithDisplayOrder(int displayOrder)
        {
            _displayOrder = displayOrder;
            return this;
        }

        public ToppingRequestDtoBuilder WithIsAvailable(bool isAvailable)
        {
            _isAvailable = isAvailable;
            return this;
        }

        public ToppingRequestDto Build()
        {
            return new ToppingRequestDto
            {
                Name = _name,
                Price = _price,
                DisplayOrder = _displayOrder,
                IsAvailable = _isAvailable
            };
        }
    }

    public class CreateToppingGroupRequestDtoBuilder
    {
        private string _name = "Cheese Options";
        private string? _description = "Choose your cheese";
        private bool _isRequired = true;
        private int _minSelections = 1;
        private int _maxSelections = 2;
        private bool _isActive = true;
        private List<ToppingRequestDto> _toppings = new();
        private List<Guid> _appliedProductIds = new();

        public CreateToppingGroupRequestDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder WithDescription(string? description)
        {
            _description = description;
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder WithIsRequired(bool isRequired)
        {
            _isRequired = isRequired;
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder WithMinSelections(int minSelections)
        {
            _minSelections = minSelections;
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder WithMaxSelections(int maxSelections)
        {
            _maxSelections = maxSelections;
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder WithToppings(List<ToppingRequestDto>? toppings)
        {
            _toppings = toppings ?? new();
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder AddTopping(ToppingRequestDto topping)
        {
            _toppings.Add(topping);
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder WithAppliedProductIds(List<Guid>? productIds)
        {
            _appliedProductIds = productIds ?? new();
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder AddAppliedProductId(Guid productId)
        {
            _appliedProductIds.Add(productId);
            return this;
        }

        public CreateToppingGroupRequestDto Build()
        {
            return new CreateToppingGroupRequestDto
            {
                Name = _name,
                Description = _description,
                IsRequired = _isRequired,
                MinSelections = _minSelections,
                MaxSelections = _maxSelections,
                IsActive = _isActive,
                Toppings = _toppings,
                AppliedProductIds = _appliedProductIds
            };
        }
    }

    public class UpdateToppingGroupRequestDtoBuilder
    {
        private string _name = "Cheese Options Updated";
        private string? _description = "Updated cheese selection";
        private bool _isRequired = true;
        private int _minSelections = 1;
        private int _maxSelections = 3;
        private bool _isActive = true;
        private List<UpdateToppingRequestDto> _toppings = new();
        private List<Guid> _appliedProductIds = new();

        public UpdateToppingGroupRequestDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder WithDescription(string? description)
        {
            _description = description;
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder WithIsRequired(bool isRequired)
        {
            _isRequired = isRequired;
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder WithMinSelections(int minSelections)
        {
            _minSelections = minSelections;
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder WithMaxSelections(int maxSelections)
        {
            _maxSelections = maxSelections;
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder WithToppings(List<UpdateToppingRequestDto>? toppings)
        {
            _toppings = toppings ?? new();
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder AddTopping(UpdateToppingRequestDto topping)
        {
            _toppings.Add(topping);
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder WithAppliedProductIds(List<Guid>? productIds)
        {
            _appliedProductIds = productIds ?? new();
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder AddAppliedProductId(Guid productId)
        {
            _appliedProductIds.Add(productId);
            return this;
        }

        public UpdateToppingGroupRequestDto Build()
        {
            return new UpdateToppingGroupRequestDto
            {
                Name = _name,
                Description = _description,
                IsRequired = _isRequired,
                MinSelections = _minSelections,
                MaxSelections = _maxSelections,
                IsActive = _isActive,
                Toppings = _toppings,
                AppliedProductIds = _appliedProductIds
            };
        }
    }
}
