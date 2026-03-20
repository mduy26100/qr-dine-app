namespace QRDine.Application.Tests.Common.Builders.Catalog
{
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
}
