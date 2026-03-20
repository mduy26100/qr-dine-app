namespace QRDine.Application.Tests.Common.Builders.Sales
{
    public class StorefrontCreateOrderDtoBuilder
    {
        private Guid _tableId = Guid.NewGuid();
        private Guid? _sessionId = null;
        private string? _customerName = null;
        private string? _customerPhone = null;
        private string? _note = null;
        private List<StorefrontCreateOrderItemDto> _items = new();

        public StorefrontCreateOrderDtoBuilder WithTableId(Guid tableId)
        {
            _tableId = tableId;
            return this;
        }

        public StorefrontCreateOrderDtoBuilder WithSessionId(Guid? sessionId)
        {
            _sessionId = sessionId;
            return this;
        }

        public StorefrontCreateOrderDtoBuilder WithCustomerName(string? customerName)
        {
            _customerName = customerName;
            return this;
        }

        public StorefrontCreateOrderDtoBuilder WithCustomerPhone(string? customerPhone)
        {
            _customerPhone = customerPhone;
            return this;
        }

        public StorefrontCreateOrderDtoBuilder WithNote(string? note)
        {
            _note = note;
            return this;
        }

        public StorefrontCreateOrderDtoBuilder WithItems(List<StorefrontCreateOrderItemDto> items)
        {
            _items = items;
            return this;
        }

        public StorefrontCreateOrderDtoBuilder AddItem(StorefrontCreateOrderItemDto item)
        {
            _items.Add(item);
            return this;
        }

        public StorefrontCreateOrderDto Build()
        {
            return new StorefrontCreateOrderDto
            {
                TableId = _tableId,
                SessionId = _sessionId,
                CustomerName = _customerName,
                CustomerPhone = _customerPhone,
                Note = _note,
                Items = _items.Count > 0 ? _items : new List<StorefrontCreateOrderItemDto>
                {
                    new StorefrontCreateOrderItemDtoBuilder().Build()
                }
            };
        }
    }
}
