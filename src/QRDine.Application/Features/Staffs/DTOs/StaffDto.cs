namespace QRDine.Application.Features.Staffs.DTOs
{
    public class StaffDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
