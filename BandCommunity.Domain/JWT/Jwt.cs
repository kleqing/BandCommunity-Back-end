namespace BandCommunity.Domain.JWT;

public class Jwt
{
    public const string JWTOptionsKey = "JWT";
	
    public string Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpiryInMinutes { get; set; }
}