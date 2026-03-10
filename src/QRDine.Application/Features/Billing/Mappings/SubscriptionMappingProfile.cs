using QRDine.Application.Features.Billing.Plans.DTOs;
using QRDine.Domain.Billing;

namespace QRDine.Application.Features.Billing.Mappings
{
    public class SubscriptionMappingProfile : Profile
    {
        public SubscriptionMappingProfile()
        {
            CreateMap<Subscription, AssignPlanResponseDto>()
                .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
