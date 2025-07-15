using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BandCommunity.Domain.Entities;
using BandCommunity.Domain.JWT;
using BandCommunity.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BandCommunity.Application.Services.Auth;

public class AuthTokenProcess : IAuthTokenProcess
{
    private readonly Jwt _jwt;
    private readonly IHttpContextAccessor _contextAccessor;

    public AuthTokenProcess(IOptions<Jwt> jwt, IHttpContextAccessor contextAccessor)
    {
        _jwt = jwt.Value;
        _contextAccessor = contextAccessor;
    }

    public (string Token, DateTime Expiry) GenerateToken(User user)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) //* Set the NameIdentifier claim to the user's ID
        };

        var expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiryInMinutes);

        var token = new JwtSecurityToken(issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return (jwtToken, expires);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token, DateTime expiry)
    {
        _contextAccessor.HttpContext?.Response.Cookies.Append(cookieName, token,
            new CookieOptions
            {
                Expires = expiry,
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
    }
    
    public void DeleteAuthTokenCookie(string key)
    {
        var context = _contextAccessor.HttpContext;
        context?.Response.Cookies.Delete(key);
    }
}