using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Identity.DTOs;
using QRDine.Application.Features.Identity.Services;
using QRDine.Domain.Tenant;
using QRDine.Infrastructure.Identity.Constants;
using QRDine.Infrastructure.Identity.Models;
using QRDine.Infrastructure.Persistence;

namespace QRDine.Infrastructure.Identity.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public RegisterService(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<RegisterResponseDto> RegisterMerchantAsync(RegisterRequestDto request, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new ConflictException("This email address has already been registered.");
            }

            if (!string.IsNullOrEmpty(request.UserPhoneNumber))
            {
                var existingPhone = await _dbContext.Users
                    .AnyAsync(u => u.PhoneNumber == request.UserPhoneNumber, cancellationToken);
                if (existingPhone)
                {
                    throw new ConflictException("This phone number has already been registered.");
                }
            }

            await using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);

            try
            {
                var merchant = new Merchant
                {
                    Id = Guid.NewGuid(),
                    Name = request.MerchantName,
                    Slug = GenerateSlug(request.MerchantName),
                    Address = request.MerchantAddress,
                    PhoneNumber = request.MerchantPhoneNumber,
                    IsActive = true
                };

                _dbContext.Merchants.Add(merchant);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.UserPhoneNumber,
                    MerchantId = merchant.Id,
                    EmailConfirmed = true
                };

                var createUserResult = await _userManager.CreateAsync(user, request.Password);
                if (!createUserResult.Succeeded)
                {
                    var firstError = createUserResult.Errors.FirstOrDefault()?.Description;
                    throw new BusinessRuleException(firstError ?? "Error creating account.");
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(user, SystemRoles.Merchant);
                if (!addToRoleResult.Succeeded)
                {
                    throw new BusinessRuleException("Error when assigning account permissions.");
                }

                await transaction.CommitAsync(cancellationToken);

                return new RegisterResponseDto
                {
                    MerchantId = merchant.Id,
                    MerchantName = merchant.Name,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            string str = text.ToLowerInvariant();

            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", "-");
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\-]", "");
            return str.Trim('-');
        }
    }
}
