namespace QRDine.Application.Tests.Common.Builders.Catalog
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
}
