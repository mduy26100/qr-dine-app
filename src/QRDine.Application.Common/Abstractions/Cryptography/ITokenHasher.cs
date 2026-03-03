namespace QRDine.Application.Common.Abstractions.Cryptography
{
    public interface ITokenHasher
    {
        string HashToken(string token);
    }
}
