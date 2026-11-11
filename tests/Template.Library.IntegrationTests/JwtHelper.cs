// Copyright © Korshun Ivan. All rights reserved.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Template.Library.IntegrationTests;

public class JwtHelper
{
    private readonly SigningCredentials _signingCredentials;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtHelper()
    {
        byte[] key = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(key);

        SecurityKey = new SymmetricSecurityKey(key) { KeyId = Guid.NewGuid().ToString() };
        _signingCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);

        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public SecurityKey SecurityKey { get; }

    public string GenerateToken(string issuer, string audience, params Claim[] claims)
    {
        return GenerateToken(issuer, audience, DateTime.UtcNow.AddMinutes(20), claims);
    }

    public string GenerateToken(string issuer, string audience, DateTime expirationDate, params Claim[] claims)
    {
        return _tokenHandler.WriteToken(new JwtSecurityToken(
            issuer,
            audience,
            claims.Append(new Claim(ClaimTypes.NameIdentifier, "test-identity")),
            null,
            expirationDate,
            _signingCredentials));
    }
}
