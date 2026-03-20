namespace QRDine.Application.Tests.Common.Builders
{
    public class PlanBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _name = "Pro Plan";
        private string _code = "PRO";
        private decimal _price = 99000m;
        private int _durationDays = 30;
        private bool _isActive = true;

        public PlanBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public PlanBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public PlanBuilder WithCode(string code)
        {
            _code = code;
            return this;
        }

        public PlanBuilder WithPrice(decimal price)
        {
            _price = price;
            return this;
        }

        public PlanBuilder WithDurationDays(int days)
        {
            _durationDays = days;
            return this;
        }

        public PlanBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public Plan Build()
        {
            return new Plan
            {
                Id = _id,
                Name = _name,
                Code = _code,
                Price = _price,
                DurationDays = _durationDays,
                IsActive = _isActive
            };
        }
    }

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

    public class FeatureLimitCheckDtoBuilder
    {
        private int? _maxTables = 10;
        private int? _maxProducts = 100;
        private int? _maxStaffMembers = 5;
        private bool _allowAdvancedReports = true;

        public FeatureLimitCheckDtoBuilder WithMaxTables(int? maxTables)
        {
            _maxTables = maxTables;
            return this;
        }

        public FeatureLimitCheckDtoBuilder WithMaxProducts(int? maxProducts)
        {
            _maxProducts = maxProducts;
            return this;
        }

        public FeatureLimitCheckDtoBuilder WithMaxStaffMembers(int? maxStaffMembers)
        {
            _maxStaffMembers = maxStaffMembers;
            return this;
        }

        public FeatureLimitCheckDtoBuilder WithAllowAdvancedReports(bool allowAdvancedReports)
        {
            _allowAdvancedReports = allowAdvancedReports;
            return this;
        }

        public FeatureLimitCheckDto Build()
        {
            return new FeatureLimitCheckDto
            {
                MaxTables = _maxTables,
                MaxProducts = _maxProducts,
                MaxStaffMembers = _maxStaffMembers,
                AllowAdvancedReports = _allowAdvancedReports
            };
        }
    }

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
