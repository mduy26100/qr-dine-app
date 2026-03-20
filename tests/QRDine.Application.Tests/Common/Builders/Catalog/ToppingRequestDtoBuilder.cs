namespace QRDine.Application.Tests.Common.Builders.Catalog
{
    public class ToppingRequestDtoBuilder
    {
        private string _name = "Extra Cheese";
        private decimal _price = 50m;
        private int _displayOrder = 1;
        private bool _isAvailable = true;

        public ToppingRequestDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ToppingRequestDtoBuilder WithPrice(decimal price)
        {
            _price = price;
            return this;
        }

        public ToppingRequestDtoBuilder WithDisplayOrder(int displayOrder)
        {
            _displayOrder = displayOrder;
            return this;
        }

        public ToppingRequestDtoBuilder WithIsAvailable(bool isAvailable)
        {
            _isAvailable = isAvailable;
            return this;
        }

        public ToppingRequestDto Build()
        {
            return new ToppingRequestDto
            {
                Name = _name,
                Price = _price,
                DisplayOrder = _displayOrder,
                IsAvailable = _isAvailable
            };
        }
    }
}
