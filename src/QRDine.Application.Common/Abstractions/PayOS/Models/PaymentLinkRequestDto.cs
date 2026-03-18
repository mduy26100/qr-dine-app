namespace QRDine.Application.Common.Abstractions.PayOS.Models
{
    public class PaymentLinkRequestDto
    {
        public long OrderCode { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; } = default!;
        public string CancelUrl { get; set; } = default!;
        public string ReturnUrl { get; set; } = default!;
    }
}
