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
            await ValidateUserAndMerchantUniquenessAsync(request.Email, request.UserPhoneNumber, request.MerchantPhoneNumber, cancellationToken);

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
                    request.UserPhoneNumber, merchant.Id, SystemRoles.Merchant);

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
            await ValidateUserAndMerchantUniquenessAsync(request.Email, request.PhoneNumber, null, cancellationToken);

            var user = await CreateIdentityUserAsync(
                request.Email, request.Password, request.FirstName, request.LastName,
                request.PhoneNumber, merchantId, SystemRoles.Staff);

            return new RegisterResponseDto
            {
                MerchantId = merchantId,
                MerchantName = string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty
            };
        }

        public async Task ValidateNewMerchantAsync(RegisterMerchantDto request, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new ConflictException("Địa chỉ email này đã được đăng ký trong hệ thống.");

            var phonesToCheck = new[] { request.UserPhoneNumber, request.MerchantPhoneNumber }
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .ToList();

            if (phonesToCheck.Any())
            {
                var phoneExistsInUsers = await _dbContext.Users
                    .AnyAsync(u => phonesToCheck.Contains(u.PhoneNumber), cancellationToken);
                if (phoneExistsInUsers)
                    throw new ConflictException("Số điện thoại này đã được gắn với một tài khoản người dùng khác.");

                var phoneExistsInMerchants = await _dbContext.Merchants
                    .AnyAsync(m => phonesToCheck.Contains(m.PhoneNumber), cancellationToken);
                if (phoneExistsInMerchants)
                    throw new ConflictException("Số điện thoại này đã được đăng ký cho một cửa hàng khác.");
            }
        }

        private async Task ValidateUserAndMerchantUniquenessAsync(string email, string? userPhone, string? merchantPhone, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
                throw new ConflictException("Địa chỉ email này đã được đăng ký trong hệ thống.");

            var phonesToCheck = new List<string?>();
            if (!string.IsNullOrWhiteSpace(userPhone)) phonesToCheck.Add(userPhone);
            if (!string.IsNullOrWhiteSpace(merchantPhone)) phonesToCheck.Add(merchantPhone);

            phonesToCheck = phonesToCheck.Distinct().ToList();

            if (phonesToCheck.Any())
            {
                var existingUserPhone = await _dbContext.Users
                    .AnyAsync(u => phonesToCheck.Contains(u.PhoneNumber), cancellationToken);

                if (existingUserPhone)
                    throw new ConflictException("Số điện thoại này đã được gắn với một tài khoản người dùng khác.");

                var existingMerchantPhone = await _dbContext.Merchants
                    .AnyAsync(m => phonesToCheck.Contains(m.PhoneNumber), cancellationToken);

                if (existingMerchantPhone)
                    throw new ConflictException("Số điện thoại này đã được đăng ký cho một cửa hàng khác.");
            }
        }

        private async Task<ApplicationUser> CreateIdentityUserAsync(
            string email, string password, string firstName, string lastName,
            string? phoneNumber, Guid merchantId, string role)
        {
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
                throw new BusinessRuleException(firstError ?? "Lỗi trong quá trình tạo tài khoản.");
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, role);
            if (!addToRoleResult.Succeeded)
                throw new BusinessRuleException("Lỗi trong quá trình phân quyền tài khoản.");

            return user;
        }

        public string GenerateActivationLink(string token)
        {
            var baseUrl = _frontendSettings.BaseUrl.TrimEnd('/');
            return $"{baseUrl}/verify-email?token={token}";
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
    }
}