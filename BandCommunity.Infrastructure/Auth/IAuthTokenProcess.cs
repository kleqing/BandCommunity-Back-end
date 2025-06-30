using BandCommunity.Domain.Entities;

namespace BandCommunity.Infrastructure.Auth;

public interface IAuthTokenProcess
{
    (string Token, DateTime Expiry) GenerateToken(User user);
    string GenerateRefreshToken();
    void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token, DateTime expiry);
    void DeleteAuthTokenCookie(string key);
}