namespace QRDine.Application.Tests.Common.Builders.Sales
{
    public class OrderItemCreationDtoBuilder
    {
        private Guid _productId = Guid.NewGuid();
        private int _quantity = 1;
        private string? _note = null;
        private List<Guid>? _selectedToppingIds = null;

        public OrderItemCreationDtoBuilder WithProductId(Guid productId)
        {
            _productId = productId;
            return this;
        }

        public OrderItemCreationDtoBuilder WithQuantity(int quantity)
        {
            _quantity = quantity;
            return this;
        }

        public OrderItemCreationDtoBuilder WithNote(string? note)
        {
            _note = note;
            return this;
        }

        public OrderItemCreationDtoBuilder WithSelectedToppingIds(List<Guid>? toppingIds)
        {
            _selectedToppingIds = toppingIds;
            return this;
        }

        public OrderItemCreationDto Build()
        {
            return new OrderItemCreationDto
            {
                ProductId = _productId,
                Quantity = _quantity,
                Note = _note,
                SelectedToppingIds = _selectedToppingIds ?? new List<Guid>()
            };
        }
    }
}
