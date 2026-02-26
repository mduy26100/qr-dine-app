using QRDine.Application.Features.Catalog.Tables.DTOs;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Mappings
{
    public class TableMappingProfile : Profile
    {
        public TableMappingProfile()
        {
            CreateMap<Table, TableResponseDto>().ReverseMap();
        }
    }
}
