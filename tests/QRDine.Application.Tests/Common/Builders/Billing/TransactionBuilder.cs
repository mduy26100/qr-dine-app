namespace QRDine.Application.Tests.Common.Builders.Billing
{
    public class TransactionBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid _merchantId = Guid.NewGuid();
        private Guid _planId = Guid.NewGuid();
        private Guid _subscriptionId = Guid.NewGuid();
        private decimal _amount = 99000m;
        private PaymentStatus _status = PaymentStatus.Success;
        private PaymentMethod _method = PaymentMethod.BankTransfer;
        private DateTime _paidAt = DateTime.UtcNow;
        private string? _adminNote = null;

        public TransactionBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public TransactionBuilder WithMerchantId(Guid merchantId)
        {
            _merchantId = merchantId;
            return this;
        }

        public TransactionBuilder WithPlanId(Guid planId)
        {
            _planId = planId;
            return this;
        }

        public TransactionBuilder WithSubscriptionId(Guid subscriptionId)
        {
            _subscriptionId = subscriptionId;
            return this;
        }

        public TransactionBuilder WithAmount(decimal amount)
        {
            _amount = amount;
            return this;
        }

        public TransactionBuilder WithStatus(PaymentStatus status)
        {
            _status = status;
            return this;
        }

        public TransactionBuilder WithMethod(PaymentMethod method)
        {
            _method = method;
            return this;
        }

        public TransactionBuilder WithPaidAt(DateTime paidAt)
        {
            _paidAt = paidAt;
            return this;
        }

        public TransactionBuilder WithAdminNote(string? adminNote)
        {
            _adminNote = adminNote;
            return this;
        }

        public Transaction Build()
        {
            return new Transaction
            {
                Id = _id,
                MerchantId = _merchantId,
                PlanId = _planId,
                SubscriptionId = _subscriptionId,
                Amount = _amount,
                Status = _status,
                Method = _method,
                PaidAt = _paidAt,
                AdminNote = _adminNote
            };
        }
    }
}
