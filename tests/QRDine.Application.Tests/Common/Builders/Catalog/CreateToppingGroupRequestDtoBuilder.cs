namespace QRDine.Application.Tests.Common.Builders.Catalog
{
    public class CreateToppingGroupRequestDtoBuilder
    {
        private string _name = "Cheese Options";
        private string? _description = "Choose your cheese";
        private bool _isRequired = true;
        private int _minSelections = 1;
        private int _maxSelections = 2;
        private bool _isActive = true;
        private List<ToppingRequestDto> _toppings = new();
        private List<Guid> _appliedProductIds = new();

        public CreateToppingGroupRequestDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder WithDescription(string? description)
        {
            _description = description;
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder WithIsRequired(bool isRequired)
        {
            _isRequired = isRequired;
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder WithMinSelections(int minSelections)
        {
            _minSelections = minSelections;
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder WithMaxSelections(int maxSelections)
        {
            _maxSelections = maxSelections;
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder WithToppings(List<ToppingRequestDto>? toppings)
        {
            _toppings = toppings ?? new();
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder AddTopping(ToppingRequestDto topping)
        {
            _toppings.Add(topping);
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder WithAppliedProductIds(List<Guid>? productIds)
        {
            _appliedProductIds = productIds ?? new();
            return this;
        }

        public CreateToppingGroupRequestDtoBuilder AddAppliedProductId(Guid productId)
        {
            _appliedProductIds.Add(productId);
            return this;
        }

        public CreateToppingGroupRequestDto Build()
        {
            return new CreateToppingGroupRequestDto
            {
                Name = _name,
                Description = _description,
                IsRequired = _isRequired,
                MinSelections = _minSelections,
                MaxSelections = _maxSelections,
                IsActive = _isActive,
                Toppings = _toppings,
                AppliedProductIds = _appliedProductIds
            };
        }
    }
}
