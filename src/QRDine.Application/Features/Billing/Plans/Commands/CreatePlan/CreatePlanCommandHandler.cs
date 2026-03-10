using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Billing.Plans.DTOs;
using QRDine.Application.Features.Billing.Plans.Specifications;
using QRDine.Application.Features.Billing.Repositories;
using QRDine.Domain.Billing;

namespace QRDine.Application.Features.Billing.Plans.Commands.CreatePlan
{
    public class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, PlanResponseDto>
    {
        private readonly IPlanRepository _planRepository;
        private readonly IMapper _mapper;

        public CreatePlanCommandHandler(
            IPlanRepository planRepository,
            IMapper mapper)
        {
            _planRepository = planRepository;
            _mapper = mapper;
        }

        public async Task<PlanResponseDto> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var spec = new GetPlanByCodeSpec(dto.Code);
            var existingPlan = await _planRepository.SingleOrDefaultAsync(spec, cancellationToken);

            if (existingPlan != null)
            {
                throw new ConflictException($"Mã gói '{dto.Code}' đã tồn tại trong hệ thống. Vui lòng chọn mã khác!");
            }

            var plan = _mapper.Map<Plan>(dto);

            await _planRepository.AddAsync(plan, cancellationToken);

            return _mapper.Map<PlanResponseDto>(plan);
        }
    }
}
