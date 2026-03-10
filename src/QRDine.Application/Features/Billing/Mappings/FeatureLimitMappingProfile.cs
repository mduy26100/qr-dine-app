using QRDine.Application.Features.Billing.Plans.DTOs;
using QRDine.Domain.Billing;

namespace QRDine.Application.Features.Billing.Mappings
{
    public class FeatureLimitMappingProfile : Profile
    {
        public FeatureLimitMappingProfile()
        {
            CreateMap<FeatureLimitDto, FeatureLimit>().ReverseMap();
        }
    }
}
