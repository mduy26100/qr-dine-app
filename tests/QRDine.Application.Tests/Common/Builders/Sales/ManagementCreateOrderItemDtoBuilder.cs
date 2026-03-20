namespace QRDine.Application.Tests.Common.Builders.Sales
{
    public class ManagementCreateOrderItemDtoBuilder
    {
        private Guid _productId = Guid.NewGuid();
        private int _quantity = 1;
        private string? _note = null;
        private List<Guid>? _selectedToppingIds = null;

        public ManagementCreateOrderItemDtoBuilder WithProductId(Guid productId)
        {
            _productId = productId;
            return this;
        }

        public ManagementCreateOrderItemDtoBuilder WithQuantity(int quantity)
        {
            _quantity = quantity;
            return this;
        }

        public ManagementCreateOrderItemDtoBuilder WithNote(string? note)
        {
            _note = note;
            return this;
        }

        public ManagementCreateOrderItemDtoBuilder WithSelectedToppingIds(List<Guid>? toppingIds)
        {
            _selectedToppingIds = toppingIds;
            return this;
        }

        public ManagementCreateOrderItemDto Build()
        {
            return new ManagementCreateOrderItemDto
            {
                ProductId = _productId,
                Quantity = _quantity,
                Note = _note,
                SelectedToppingIds = _selectedToppingIds ?? new List<Guid>()
            };
        }
    }
}
