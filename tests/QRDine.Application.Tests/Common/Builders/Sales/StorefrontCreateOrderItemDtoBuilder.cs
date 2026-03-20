namespace QRDine.Application.Tests.Common.Builders.Sales
{
    public class StorefrontCreateOrderItemDtoBuilder
    {
        private Guid _productId = Guid.NewGuid();
        private int _quantity = 1;
        private string? _note = null;
        private List<Guid>? _selectedToppingIds = null;

        public StorefrontCreateOrderItemDtoBuilder WithProductId(Guid productId)
        {
            _productId = productId;
            return this;
        }

        public StorefrontCreateOrderItemDtoBuilder WithQuantity(int quantity)
        {
            _quantity = quantity;
            return this;
        }

        public StorefrontCreateOrderItemDtoBuilder WithNote(string? note)
        {
            _note = note;
            return this;
        }

        public StorefrontCreateOrderItemDtoBuilder WithSelectedToppingIds(List<Guid>? toppingIds)
        {
            _selectedToppingIds = toppingIds;
            return this;
        }

        public StorefrontCreateOrderItemDto Build()
        {
            return new StorefrontCreateOrderItemDto
            {
                ProductId = _productId,
                Quantity = _quantity,
                Note = _note,
                SelectedToppingIds = _selectedToppingIds ?? new List<Guid>()
            };
        }
    }
}
