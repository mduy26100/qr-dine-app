namespace QRDine.Application.Tests.Common.Builders.Sales
{
    public class OrderItemBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid _orderId = Guid.NewGuid();
        private Guid _productId = Guid.NewGuid();
        private string _productName = "Cơm Gà";
        private decimal _unitPrice = 50000m;
        private string? _toppingsSnapshot = null;
        private int _quantity = 1;
        private decimal _amount = 50000m;
        private OrderItemStatus _status = OrderItemStatus.Pending;
        private string? _note = null;
        private bool _isDeleted = false;

        public OrderItemBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public OrderItemBuilder WithOrderId(Guid orderId)
        {
            _orderId = orderId;
            return this;
        }

        public OrderItemBuilder WithProductId(Guid productId)
        {
            _productId = productId;
            return this;
        }

        public OrderItemBuilder WithProductName(string productName)
        {
            _productName = productName;
            return this;
        }

        public OrderItemBuilder WithUnitPrice(decimal unitPrice)
        {
            _unitPrice = unitPrice;
            return this;
        }

        public OrderItemBuilder WithToppingsSnapshot(string? toppingsSnapshot)
        {
            _toppingsSnapshot = toppingsSnapshot;
            return this;
        }

        public OrderItemBuilder WithQuantity(int quantity)
        {
            _quantity = quantity;
            return this;
        }

        public OrderItemBuilder WithAmount(decimal amount)
        {
            _amount = amount;
            return this;
        }

        public OrderItemBuilder WithStatus(OrderItemStatus status)
        {
            _status = status;
            return this;
        }

        public OrderItemBuilder WithNote(string? note)
        {
            _note = note;
            return this;
        }

        public OrderItemBuilder WithIsDeleted(bool isDeleted)
        {
            _isDeleted = isDeleted;
            return this;
        }

        public OrderItem Build()
        {
            return new OrderItem
            {
                Id = _id,
                OrderId = _orderId,
                ProductId = _productId,
                ProductName = _productName,
                UnitPrice = _unitPrice,
                ToppingsSnapshot = _toppingsSnapshot,
                Quantity = _quantity,
                Amount = _amount,
                Status = _status,
                Note = _note,
                IsDeleted = _isDeleted
            };
        }
    }
}
