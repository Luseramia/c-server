using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Text.Json;
using MySql.Data.MySqlClient;
using RedisServices;
using JwtGen;
using System.Data;
using System.Security.Claims;
namespace Controllers
{
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public UserController(ITokenService tokenService, IConfiguration configuration)
    {
        _tokenService = tokenService;
        _configuration = configuration;
    }
        public class LoginModel
        {
            public string username { get; set; }
            public string password { get; set; }
        }
    

    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromBody] LoginModel dto)
    {
        string connectionString = _configuration.GetConnectionString("DefaultConnection");
        using MySqlConnection connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        string sql = "SELECT * FROM user WHERE username = @username AND password = @password";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@username", dto.username);
        command.Parameters.AddWithValue("@password", dto.password);

        using var reader = await command.ExecuteReaderAsync();
        if (reader.HasRows && await reader.ReadAsync())
        {
            var user = new 
            {
                userId = reader.GetString("userId"),
                username = reader.GetString("username"),
                name = reader.GetString("name"),
                role = reader.GetString("role")
            };

            var token = await JwtGen.JwtGen.GenerateJwtTokenAsync(user.userId, user.role, _tokenService);

            Response.Cookies.Append("jwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(30)
            });

            return Ok(user);
        }

        return Unauthorized("Invalid credentials");
    }
        [HttpPost("/logout")]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(token))
            {
                // ตรวจสอบ token
                var principal = JwtGen.JwtGen.JwtDecoder.ValidateToken(token);
                if (principal != null)
                {
                    var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        // เพิกถอน token ใน Redis
                        await JwtGen.JwtGen.RevokeTokenAsync(userId, token, _tokenService);
                    }
                }
                
                // ลบ cookie
                Response.Cookies.Delete("jwtToken", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // ตั้งเป็น true ในสภาพแวดล้อม production และใช้ HTTPS
                    SameSite = SameSiteMode.Lax
                });
            }
            
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("/verify")]
        public async Task<IActionResult> VerifyToken()
        {
            var token = Request.Cookies["jwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "No token found" });
            }
            
            // ตรวจสอบ token
            var principal = JwtGen.JwtGen.JwtDecoder.ValidateToken(token);
            if (principal == null)
            {
                return Unauthorized(new { message = "Invalid token" });
            }
            
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token claims" });
            }
            
            // ตรวจสอบว่า token ยังมีอยู่ใน Redis หรือไม่
            if (!await _tokenService.IsTokenValid(userId, token))
            {
                return Unauthorized(new { message = "Token has been revoked" });
            }
            
            return Ok(new
            {
                userId = userId,
                username = principal.FindFirstValue(ClaimTypes.Name),
                role = principal.FindFirstValue(ClaimTypes.Role)
            });
        }
        [HttpPost("/logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            var token = Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(token))
            {
                // ตรวจสอบ token
                var principal = JwtGen.JwtGen.JwtDecoder.ValidateToken(token);
                if (principal != null)
                {
                    var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        // เพิกถอน token ทั้งหมดของ user ใน Redis
                        await JwtGen.JwtGen.RevokeAllTokensAsync(userId, _tokenService);
                    }
                }
                
                // ลบ cookie
                Response.Cookies.Delete("jwtToken");
            }
            
            return Ok(new { message = "Logged out from all devices successfully" });
        }


}
}