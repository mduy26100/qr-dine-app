using SharedKernel.Application.Interfaces.Payment.Models;

namespace SharedKernel.Application.Interfaces.Payment
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentLinkAsync(PaymentLinkRequestDto request);
    }
}
