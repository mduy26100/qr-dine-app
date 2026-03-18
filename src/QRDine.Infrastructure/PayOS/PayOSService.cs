using QRDine.Application.Common.Abstractions.PayOS;
using QRDine.Application.Common.Abstractions.PayOS.Models;

namespace QRDine.Infrastructure.PayOS
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOSClient _payOSClient;

        public PayOSService(PayOSClient payOSClient)
        {
            _payOSClient = payOSClient;
        }

        public async Task<string> CreatePaymentLinkAsync(PaymentLinkRequestDto request)
        {
            var paymentData = new CreatePaymentLinkRequest
            {
                OrderCode = request.OrderCode,
                Amount = request.Amount,
                Description = request.Description,
                CancelUrl = request.CancelUrl,
                ReturnUrl = request.ReturnUrl
            };

            var paymentLink = await _payOSClient.PaymentRequests.CreateAsync(paymentData);

            return paymentLink.CheckoutUrl;
        }
    }
}
