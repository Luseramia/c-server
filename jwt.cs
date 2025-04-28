using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Claims;
using RedisServices;
namespace JwtGen;

public class JwtGen()
{
    public static async Task<string> GenerateJwtTokenAsync(string userId, string role, ITokenService tokenService)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key_which_is_16_bytes")); // คีย์ลับ
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Name, userId),
        new Claim(ClaimTypes.Role, role),  
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        // new Claim(JwtRegisteredClaimNames.Sub, "userId"),
        // new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };
        var expiry = TimeSpan.FromMinutes(30);
        var expiryDateTime = DateTime.Now.Add(expiry);
        var token = new JwtSecurityToken(
            issuer: "tarchunk.win",
            audience: "tarchunk.win",
            claims: claims,
            expires: expiryDateTime,
            signingCredentials: credentials
        );
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        await tokenService.SaveToken(userId, tokenString, expiry);

        return tokenString;
    }
    public static async Task RevokeTokenAsync(string userId, string token, ITokenService tokenService)
    {
        await tokenService.RevokeToken(userId, token);
    }

    // ลบ token ทั้งหมดของ user จาก Redis (logout from all devices)
    public static async Task RevokeAllTokensAsync(string userId, ITokenService tokenService)
    {
        await tokenService.RevokeAllUserTokens(userId);
    }

public class JwtDecoder
    {
        public static bool DecodeJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // ตรวจสอบว่า token นี้สามารถอ่านได้หรือไม่
            if (!tokenHandler.CanReadToken(token))
            {
                Console.WriteLine("Invalid token");
                return false;
            }
            // แปลง token string เป็น JwtSecurityToken object
            // var jwtToken = tokenHandler.ReadJwtToken(token);
            // อ่าน claims จาก payload
            // foreach (var claim in jwtToken.Claims)
            // {
            //     Console.WriteLine($"{claim.Type}: {claim.Value}");
            // }

            // แสดงรายละเอียดอื่น ๆ เช่น issuer และ audience
            // Console.WriteLine($"Issuer: {jwtToken.Issuer}");
            // Console.WriteLine($"Audience: {jwtToken.Audiences}");
            // Console.WriteLine($"Expires: {jwtToken.ValidTo}");
            return true;
        }
        public static ClaimsPrincipal ValidateToken(string token)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes("your_secret_key_which_is_16_bytes");
    
    try
    {
        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = "tarchunk.win",
            ValidateAudience = true,
            ValidAudience = "tarchunk.win",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out _);
        
        return principal;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Token validation failed: {ex.Message}");
        return null;
    }
}
    }
}
