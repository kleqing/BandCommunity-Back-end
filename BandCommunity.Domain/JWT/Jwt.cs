﻿using System.Diagnostics.CodeAnalysis;

namespace BandCommunity.Domain.JWT;

public class Jwt
{
    public const string JwtOptionsKey = "JWT";

    public required string Secret { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int ExpiryInMinutes { get; set; }
}