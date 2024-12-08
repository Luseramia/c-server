using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtGen;

public class JwtGen()
{
    public static string GenerateJwtToken()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key_which_is_16_bytes")); // คีย์ลับ
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, "userId"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var token = new JwtSecurityToken(
            issuer: "yourdomain.com",
            audience: "yourdomain.com",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token); // return token เป็น string
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
    }
}
