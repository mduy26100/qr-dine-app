namespace SharedKernel.Payment.Services
{
    public class PayOSPaymentService : IPaymentService
    {
        private readonly PayOSClient _payOSClient;

        public PayOSPaymentService(PayOSClient payOSClient)
        {
            _payOSClient = payOSClient;
        }

        public async Task<string> CreatePaymentLinkAsync(PaymentLinkRequestDto request)
        {
            if (!long.TryParse(request.TransactionReference, out long orderCode))
            {
                throw new ArgumentException("PayOS requires a numeric TransactionReference (OrderCode).", nameof(request.TransactionReference));
            }

            var paymentData = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = (int)request.Amount,
                Description = request.Description,
                CancelUrl = request.CancelUrl,
                ReturnUrl = request.ReturnUrl
            };

            var paymentLink = await _payOSClient.PaymentRequests.CreateAsync(paymentData);

            return paymentLink.CheckoutUrl;
        }
    }
}
