namespace QRDine.Application.Tests.Common.Builders.Sales
{
    public class OrderBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid _merchantId = Guid.NewGuid();
        private Guid _tableId = Guid.NewGuid();
        private string _tableName = "Bàn 1";
        private string _orderCode = "ORD001";
        private Guid _sessionId = Guid.NewGuid();
        private OrderStatus _status = OrderStatus.Open;
        private decimal _totalAmount = 0m;
        private string? _note = null;
        private string? _customerName = null;
        private string? _customerPhone = null;
        private bool _isDeleted = false;
        private List<OrderItem> _orderItems = new();

        public OrderBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public OrderBuilder WithMerchantId(Guid merchantId)
        {
            _merchantId = merchantId;
            return this;
        }

        public OrderBuilder WithTableId(Guid tableId)
        {
            _tableId = tableId;
            return this;
        }

        public OrderBuilder WithTableName(string tableName)
        {
            _tableName = tableName;
            return this;
        }

        public OrderBuilder WithOrderCode(string orderCode)
        {
            _orderCode = orderCode;
            return this;
        }

        public OrderBuilder WithSessionId(Guid sessionId)
        {
            _sessionId = sessionId;
            return this;
        }

        public OrderBuilder WithStatus(OrderStatus status)
        {
            _status = status;
            return this;
        }

        public OrderBuilder WithTotalAmount(decimal totalAmount)
        {
            _totalAmount = totalAmount;
            return this;
        }

        public OrderBuilder WithNote(string? note)
        {
            _note = note;
            return this;
        }

        public OrderBuilder WithCustomerName(string? customerName)
        {
            _customerName = customerName;
            return this;
        }

        public OrderBuilder WithCustomerPhone(string? customerPhone)
        {
            _customerPhone = customerPhone;
            return this;
        }

        public OrderBuilder WithIsDeleted(bool isDeleted)
        {
            _isDeleted = isDeleted;
            return this;
        }

        public OrderBuilder WithOrderItems(List<OrderItem> orderItems)
        {
            _orderItems = orderItems;
            return this;
        }

        public Order Build()
        {
            return new Order
            {
                Id = _id,
                MerchantId = _merchantId,
                TableId = _tableId,
                TableName = _tableName,
                OrderCode = _orderCode,
                SessionId = _sessionId,
                Status = _status,
                TotalAmount = _totalAmount,
                Note = _note,
                CustomerName = _customerName,
                CustomerPhone = _customerPhone,
                IsDeleted = _isDeleted,
                OrderItems = _orderItems
            };
        }
    }
}
