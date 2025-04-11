using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.IdentityModel.Tokens;
using models.User;
using MySql.Data.MySqlClient;
using Routes;

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
});

builder.Services.AddAuthorization();
// builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
// builder.Services.AddSingleton<DatabaseService>();

var app = builder.Build();
app.ConfigureRoutes();
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key_which_is_16_bytes"));
var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

app.Use(async (context, next) =>
{
  var token = context.Request.Cookies["jwtToken"]; // อ่าน jwt จาก cookie
  Console.WriteLine(token);
  if (!string.IsNullOrEmpty(token))
  {
    // เพิ่ม Authorization Header ในคำขอ
    context.Request.Headers.Add("Authorization", $"Bearer {token}");
  }


  await next(); // เรียก middleware ถัดไป
});


var token = new JwtSecurityToken(
    issuer: "localhost",
    audience: "localhost",
    expires: DateTime.Now.AddMinutes(30),
    signingCredentials: creds);

var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
Console.WriteLine(tokenString);

app.MapGet("/test", [Authorize] (HttpContext context) =>
{

  using var reader = new StreamReader(context.Request.Body);
  var test = context.Request.Headers;
  Console.WriteLine($"{test}");
  return Results.Ok("testsuc");
});

app.MapGet("/test1", [Authorize] async (HttpContext context) =>
{
  using var reader = new StreamReader(context.Request.Body);
  IHeaderDictionary test = context.Request.Headers;
  Console.WriteLine(test.Count);
  var body = await reader.ReadToEndAsync();
  Console.WriteLine("body=", body);
});
app.UseAuthentication();
app.UseAuthorization();
app.UseCors(MyAllowSpecificOrigins);
app.Run();


public class MyData
{
  public string SomeProperty { get; set; }
}