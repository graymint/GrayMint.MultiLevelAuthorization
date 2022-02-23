using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace MultiLevelAuthorization.Server;

public class JwtTool
{
    public static string CreateSymmetricJwt(byte[] secret, string issuer, string audience, string subject,
        Claim[]? claims = null, string[]? roles = null)
    {
        var claimsList = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        if (claims != null) claimsList.AddRange(claims);
        if (roles != null) claimsList.AddRange(roles.Select(x=>new Claim(ClaimTypes.Role, x)));

        // create token
        var secKey = new SymmetricSecurityKey(secret);
        var signingCredentials = new SigningCredentials(secKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer,
            claims: claimsList.ToArray(),
            audience: audience,
            expires: DateTime.Now.AddYears(10),
            signingCredentials: signingCredentials);

        var handler = new JwtSecurityTokenHandler();
        var ret = handler.WriteToken(token);
        return ret;
    }
}