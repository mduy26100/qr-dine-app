using QRDine.Application.Common.Abstractions.PayOS.Models;

namespace QRDine.Application.Common.Abstractions.PayOS
{
    public interface IPayOSService
    {
        Task<string> CreatePaymentLinkAsync(PaymentLinkRequestDto request);
    }
}
