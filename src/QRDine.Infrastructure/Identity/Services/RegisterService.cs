using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Identity.DTOs;
using QRDine.Application.Features.Identity.Services;
using QRDine.Domain.Tenant;
using QRDine.Infrastructure.Configuration;
using QRDine.Infrastructure.Identity.Constants;
using QRDine.Infrastructure.Identity.Models;
using QRDine.Infrastructure.Persistence;

namespace QRDine.Infrastructure.Identity.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly FrontendSettings _frontendSettings;

        public RegisterService(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, IOptions<FrontendSettings> options)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _frontendSettings = options.Value;
        }

        public async Task<RegisterResponseDto> ConfirmMerchantRegistrationAsync(RegisterMerchantDto request, CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);

            try
            {
                var merchant = new Merchant
                {
                    Id = Guid.NewGuid(),
                    Name = request.MerchantName,
                    Slug = await GenerateUniqueSlugAsync(request.MerchantName, cancellationToken),
                    Address = request.MerchantAddress,
                    PhoneNumber = request.MerchantPhoneNumber,
                    IsActive = true
                };

                _dbContext.Merchants.Add(merchant);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var user = await CreateIdentityUserAsync(
                    request.Email, request.Password, request.FirstName, request.LastName,
                    request.UserPhoneNumber, merchant.Id, SystemRoles.Merchant, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return new RegisterResponseDto
                {
                    MerchantId = merchant.Id,
                    MerchantName = merchant.Name,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<RegisterResponseDto> RegisterStaffAsync(RegisterStaffDto request, Guid merchantId, CancellationToken cancellationToken)
        {
            var user = await CreateIdentityUserAsync(
                request.Email, request.Password, request.FirstName, request.LastName,
                request.PhoneNumber, merchantId, SystemRoles.Staff, cancellationToken);

            return new RegisterResponseDto
            {
                MerchantId = merchantId,
                MerchantName = string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty
            };
        }

        private async Task<ApplicationUser> CreateIdentityUserAsync(
            string email, string password, string firstName, string lastName,
            string? phoneNumber, Guid merchantId, string role, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
                throw new ConflictException("This email address has already been registered.");

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                var existingPhone = await _dbContext.Users
                    .AnyAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
                if (existingPhone)
                    throw new ConflictException("This phone number has already been registered.");
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                MerchantId = merchantId,
                EmailConfirmed = true
            };

            var createUserResult = await _userManager.CreateAsync(user, password);
            if (!createUserResult.Succeeded)
            {
                var firstError = createUserResult.Errors.FirstOrDefault()?.Description;
                throw new BusinessRuleException(firstError ?? "Error creating account.");
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, role);
            if (!addToRoleResult.Succeeded)
                throw new BusinessRuleException("Error when assigning account permissions.");

            return user;
        }


        private async Task<string> GenerateUniqueSlugAsync(string merchantName, CancellationToken cancellationToken)
        {
            var baseSlug = GenerateSlug(merchantName);

            var existingSlugs = await _dbContext.Merchants
                .Where(m => m.Slug.StartsWith(baseSlug))
                .Select(m => m.Slug)
                .ToListAsync(cancellationToken);

            if (!existingSlugs.Contains(baseSlug))
            {
                return baseSlug;
            }

            int counter = 1;
            string uniqueSlug;

            do
            {
                uniqueSlug = $"{baseSlug}-{counter}";
                counter++;
            }
            while (existingSlugs.Contains(uniqueSlug));

            return uniqueSlug;
        }

        private static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            text = text.ToLowerInvariant();

            text = RemoveVietnameseDiacritics(text);

            text = System.Text.RegularExpressions.Regex.Replace(text, @"[^a-z0-9\s-]", "");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", "-").Trim('-');

            return text;
        }

        private static string RemoveVietnameseDiacritics(string text)
        {
            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                {
                    text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
                }
            }

            return text;
        }

        public async Task ValidateNewMerchantAsync(string email, string? phoneNumber, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
                throw new ConflictException("Địa chỉ email này đã được đăng ký trong hệ thống.");

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                var existingPhone = await _dbContext.Users
                    .AnyAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
                if (existingPhone)
                    throw new ConflictException("Số điện thoại này đã được sử dụng.");
            }
        }

        public string GenerateActivationLink(string token)
        {
            var baseUrl = _frontendSettings.BaseUrl.TrimEnd('/');
            return $"{baseUrl}/verify-email?token={token}";
        }
    }
}