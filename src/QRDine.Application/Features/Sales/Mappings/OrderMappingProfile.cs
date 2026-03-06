using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Sales.Mappings
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<Order, OrderResponseDto>().ReverseMap();
        }
    }
}
