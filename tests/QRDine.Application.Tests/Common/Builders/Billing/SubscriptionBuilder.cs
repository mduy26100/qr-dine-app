namespace QRDine.Application.Tests.Common.Builders.Billing
{
    public class SubscriptionBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid _merchantId = Guid.NewGuid();
        private Guid _planId = Guid.NewGuid();
        private SubscriptionStatus _status = SubscriptionStatus.Active;
        private DateTime _startDate = DateTime.UtcNow;
        private DateTime _endDate = DateTime.UtcNow.AddDays(30);
        private string? _adminNote = null;

        public SubscriptionBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public SubscriptionBuilder WithMerchantId(Guid merchantId)
        {
            _merchantId = merchantId;
            return this;
        }

        public SubscriptionBuilder WithPlanId(Guid planId)
        {
            _planId = planId;
            return this;
        }

        public SubscriptionBuilder WithStatus(SubscriptionStatus status)
        {
            _status = status;
            return this;
        }

        public SubscriptionBuilder WithStartDate(DateTime startDate)
        {
            _startDate = startDate;
            return this;
        }

        public SubscriptionBuilder WithEndDate(DateTime endDate)
        {
            _endDate = endDate;
            return this;
        }

        public SubscriptionBuilder WithAdminNote(string? adminNote)
        {
            _adminNote = adminNote;
            return this;
        }

        public Subscription Build()
        {
            return new Subscription
            {
                Id = _id,
                MerchantId = _merchantId,
                PlanId = _planId,
                Status = _status,
                StartDate = _startDate,
                EndDate = _endDate,
                AdminNote = _adminNote
            };
        }
    }
}
