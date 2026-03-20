namespace QRDine.Application.Tests.Common.Builders.Billing
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
}
