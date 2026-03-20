namespace QRDine.Application.Tests.Common.Builders.Catalog
{
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
}
