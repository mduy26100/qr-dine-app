namespace QRDine.API.Services
{
    public interface IAuthCookieService
    {
        void AppendRefreshTokenCookie(string refreshToken);
        void DeleteRefreshTokenCookie();
    }
}
