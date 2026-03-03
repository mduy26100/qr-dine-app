using QRDine.Application.Common.Abstractions.Cryptography;
using QRDine.Infrastructure.Configuration;

namespace QRDine.Infrastructure.Cryptography
{
    public class TokenHasher : ITokenHasher
    {
        private readonly string _secretKey;

        public TokenHasher(IOptions<SecuritySettings> options)
        {
            if (string.IsNullOrWhiteSpace(options.Value.TokenHashSecret))
            {
                throw new ArgumentNullException(nameof(options.Value.TokenHashSecret), "Token hash secret key is not configured.");
            }

            _secretKey = options.Value.TokenHashSecret;
        }

        public string HashToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token to hash cannot be null or empty.", nameof(token));
            }

            var keyBytes = Encoding.UTF8.GetBytes(_secretKey);
            var tokenBytes = Encoding.UTF8.GetBytes(token);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(tokenBytes);

            return Convert.ToBase64String(hashBytes);
        }
    }
}
