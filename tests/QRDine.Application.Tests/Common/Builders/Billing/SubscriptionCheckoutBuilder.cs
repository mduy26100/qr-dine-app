namespace QRDine.Application.Tests.Common.Builders.Billing
{
    public class SubscriptionCheckoutBuilder
    {
        private Guid _id = Guid.NewGuid();
        private long _orderCode = DateTime.UtcNow.Ticks;
        private Guid _merchantId = Guid.NewGuid();
        private Guid _planId = Guid.NewGuid();
        private decimal _amount = 99000m;
        private PaymentStatus _status = PaymentStatus.Pending;
        private string _planSnapshotName = "Pro Plan";
        private string? _failureReason = null;

        public SubscriptionCheckoutBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public SubscriptionCheckoutBuilder WithOrderCode(long orderCode)
        {
            _orderCode = orderCode;
            return this;
        }

        public SubscriptionCheckoutBuilder WithMerchantId(Guid merchantId)
        {
            _merchantId = merchantId;
            return this;
        }

        public SubscriptionCheckoutBuilder WithPlanId(Guid planId)
        {
            _planId = planId;
            return this;
        }

        public SubscriptionCheckoutBuilder WithAmount(decimal amount)
        {
            _amount = amount;
            return this;
        }

        public SubscriptionCheckoutBuilder WithStatus(PaymentStatus status)
        {
            _status = status;
            return this;
        }

        public SubscriptionCheckoutBuilder WithPlanSnapshotName(string planSnapshotName)
        {
            _planSnapshotName = planSnapshotName;
            return this;
        }

        public SubscriptionCheckoutBuilder WithFailureReason(string? failureReason)
        {
            _failureReason = failureReason;
            return this;
        }

        public SubscriptionCheckout Build()
        {
            return new SubscriptionCheckout
            {
                Id = _id,
                OrderCode = _orderCode,
                MerchantId = _merchantId,
                PlanId = _planId,
                Amount = _amount,
                Status = _status,
                PlanSnapshotName = _planSnapshotName,
                FailureReason = _failureReason
            };
        }
    }
}
