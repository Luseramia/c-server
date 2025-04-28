using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.IdentityModel.Tokens;
using models.User;
using MySql.Data.MySqlClient;
// using Routes;
using RedisServices;
using StackExchange.Redis;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
  options.AddPolicy(name: MyAllowSpecificOrigins,
                    policy =>
                    {
                      policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials()
                                                .AllowAnyMethod();
                      ;
                    });
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"] ?? "192.168.1.53:6379"));
builder.Services.AddSingleton<ITokenService, RedisTokenService>();


builder.Services.AddAuthentication(option =>
{
  option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
  option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{

  option.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = false,
    ValidateAudience = false,
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
    ValidateLifetime = false,
    ValidateIssuerSigningKey = true,
  };

    option.Events = new JwtBearerEvents
  {
    OnTokenValidated = async context =>
    {
      try {
        var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
        var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var token = context.SecurityToken as JwtSecurityToken;
        
        if (userId != null && token != null)
        {
          var tokenString = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
          
          // ตรวจสอบว่า token นี้อยู่ใน Redis หรือไม่
          if (!await tokenService.IsTokenValid(userId, tokenString))
          {
            context.Fail("Token has been revoked");
          }
        }
      }
      catch (Exception ex)
      {
        context.Fail($"Error validating token: {ex.Message}");
      }
    }
  };
});






builder.Services.AddAuthorization();
// builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
// builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddControllers();

var app = builder.Build();
// app.ConfigureRoutes();
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key_which_is_16_bytes"));
var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

app.Use(async (context, next) =>
{
  var token = context.Request.Cookies["jwtToken"]; // อ่าน jwt จาก cookie
  if (!string.IsNullOrEmpty(token))
  {
    // เพิ่ม Authorization Header ในคำขอ
    context.Request.Headers.Add("Authorization", $"Bearer {token}");
  }


  await next(); // เรียก middleware ถัดไป
});


app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();


public class MyData
{
  public string SomeProperty { get; set; }
}