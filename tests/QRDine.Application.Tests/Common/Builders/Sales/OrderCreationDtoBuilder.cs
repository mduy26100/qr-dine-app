namespace QRDine.Application.Tests.Common.Builders.Sales
{
    public class OrderCreationDtoBuilder
    {
        private Guid _merchantId = Guid.NewGuid();
        private Guid _tableId = Guid.NewGuid();
        private Guid? _sessionId = null;
        private string? _customerName = null;
        private string? _customerPhone = null;
        private string? _note = null;
        private List<OrderItemCreationDto> _items = new();

        public OrderCreationDtoBuilder WithMerchantId(Guid merchantId)
        {
            _merchantId = merchantId;
            return this;
        }

        public OrderCreationDtoBuilder WithTableId(Guid tableId)
        {
            _tableId = tableId;
            return this;
        }

        public OrderCreationDtoBuilder WithSessionId(Guid? sessionId)
        {
            _sessionId = sessionId;
            return this;
        }

        public OrderCreationDtoBuilder WithCustomerName(string? customerName)
        {
            _customerName = customerName;
            return this;
        }

        public OrderCreationDtoBuilder WithCustomerPhone(string? customerPhone)
        {
            _customerPhone = customerPhone;
            return this;
        }

        public OrderCreationDtoBuilder WithNote(string? note)
        {
            _note = note;
            return this;
        }

        public OrderCreationDtoBuilder WithItems(List<OrderItemCreationDto> items)
        {
            _items = items;
            return this;
        }

        public OrderCreationDtoBuilder AddItem(OrderItemCreationDto item)
        {
            _items.Add(item);
            return this;
        }

        public OrderCreationDto Build()
        {
            return new OrderCreationDto
            {
                MerchantId = _merchantId,
                TableId = _tableId,
                SessionId = _sessionId,
                CustomerName = _customerName,
                CustomerPhone = _customerPhone,
                Note = _note,
                Items = _items.Count > 0 ? _items : new List<OrderItemCreationDto>
                {
                    new OrderItemCreationDtoBuilder().Build()
                }
            };
        }
    }
}
