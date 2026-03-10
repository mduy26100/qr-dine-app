using QRDine.Application.Features.Billing.Plans.DTOs;
using QRDine.Domain.Billing;

namespace QRDine.Application.Features.Billing.Mappings
{
    public class PlanMappingProfile : Profile
    {
        public PlanMappingProfile()
        {
            CreateMap<CreatePlanDto, Plan>()
                .ForMember(dest => dest.FeatureLimit, opt => opt.MapFrom(src => src.Limits));

            CreateMap<Plan, PlanResponseDto>().ReverseMap();
        }
    }
}
