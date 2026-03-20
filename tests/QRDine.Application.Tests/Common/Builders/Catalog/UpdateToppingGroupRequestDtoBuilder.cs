namespace QRDine.Application.Tests.Common.Builders.Catalog
{
    public class UpdateToppingGroupRequestDtoBuilder
    {
        private string _name = "Cheese Options Updated";
        private string? _description = "Updated cheese selection";
        private bool _isRequired = true;
        private int _minSelections = 1;
        private int _maxSelections = 3;
        private bool _isActive = true;
        private List<UpdateToppingRequestDto> _toppings = new();
        private List<Guid> _appliedProductIds = new();

        public UpdateToppingGroupRequestDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder WithDescription(string? description)
        {
            _description = description;
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder WithIsRequired(bool isRequired)
        {
            _isRequired = isRequired;
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder WithMinSelections(int minSelections)
        {
            _minSelections = minSelections;
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder WithMaxSelections(int maxSelections)
        {
            _maxSelections = maxSelections;
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder WithToppings(List<UpdateToppingRequestDto>? toppings)
        {
            _toppings = toppings ?? new();
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder AddTopping(UpdateToppingRequestDto topping)
        {
            _toppings.Add(topping);
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder WithAppliedProductIds(List<Guid>? productIds)
        {
            _appliedProductIds = productIds ?? new();
            return this;
        }

        public UpdateToppingGroupRequestDtoBuilder AddAppliedProductId(Guid productId)
        {
            _appliedProductIds.Add(productId);
            return this;
        }

        public UpdateToppingGroupRequestDto Build()
        {
            return new UpdateToppingGroupRequestDto
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
