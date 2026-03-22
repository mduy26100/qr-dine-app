namespace QRDine.Application.Features.Identity.DTOs
{
    public class ChangePasswordRequestDto
    {
        public string OldPassword { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
        public string ConfirmNewPassword { get; set; } = default!;
    }
}
