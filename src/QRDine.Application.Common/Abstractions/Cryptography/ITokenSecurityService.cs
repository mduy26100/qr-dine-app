namespace QRDine.Application.Common.Abstractions.Cryptography
{
    public interface ITokenSecurityService
    {
        string HashToken(string token);
        bool VerifyToken(string plainToken, string hashedToken);
    }
}
