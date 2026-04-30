namespace SharedKernel.Application.Interfaces.Payment.Models
{
    public class PaymentLinkRequestDto
    {
        public string TransactionReference { get; set; } = default!;
        public long Amount { get; set; }
        public string Description { get; set; } = default!;
        public string CancelUrl { get; set; } = default!;
        public string ReturnUrl { get; set; } = default!;
    }
}
