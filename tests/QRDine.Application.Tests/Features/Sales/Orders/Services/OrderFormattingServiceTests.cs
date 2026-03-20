namespace QRDine.Application.Tests.Features.Sales.Orders.Services
{
    public class OrderFormattingServiceTests
    {
        private readonly OrderFormattingService _service;

        public OrderFormattingServiceTests()
        {
            _service = new OrderFormattingService();
        }

        [Fact]
        public void FormatToppingSnapshot_NullInput_ShouldReturnNull()
        {
            var result = _service.FormatToppingSnapshot(null);

            result.Should().BeNull();
        }

        [Fact]
        public void FormatToppingSnapshot_EmptyStringInput_ShouldReturnNull()
        {
            var result = _service.FormatToppingSnapshot("");

            result.Should().BeNull();
        }

        [Fact]
        public void FormatToppingSnapshot_WhitespaceOnlyInput_ShouldReturnNull()
        {
            var result = _service.FormatToppingSnapshot("   ");

            result.Should().BeNull();
        }

        [Fact]
        public void FormatToppingSnapshot_InvalidJsonInput_ShouldReturnNull()
        {
            var result = _service.FormatToppingSnapshot("invalid json");

            result.Should().BeNull();
        }

        [Fact]
        public void FormatToppingSnapshot_EmptyArrayJson_ShouldReturnNull()
        {
            var json = "[]";

            var result = _service.FormatToppingSnapshot(json);

            result.Should().BeNull();
        }

        [Fact]
        public void FormatToppingSnapshot_MalformedJsonArray_ShouldReturnNull()
        {
            var json = "[{\"id\":\"00000000-0000-0000-0000-000000000001\",\"name\":\"Topping\"}";

            var result = _service.FormatToppingSnapshot(json);

            result.Should().BeNull();
        }
    }
}
