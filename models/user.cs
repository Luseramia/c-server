using System.Data;
using Microsoft.AspNetCore.Authentication;
using MySql.Data.MySqlClient;
namespace models.User;

using System.Dynamic;
using System.Text.Json;
using JwtGen;
using MySqlX.XDevAPI.Common;

class User
{
    public static IResult Login(ExpandoObject body, MySqlConnection connection, HttpContext context)
    {

        dynamic data = body;
        // Console.WriteLine(data.id.ToString());
        string username = data.username.ToString();
        string password = data.password.ToString();
        // string id = body["id"];
        string sql = "SELECT * FROM member WHERE username = @username AND password = @password";
        using (MySqlCommand command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    var tokenString = JwtGen.GenerateJwtToken();
                    context.Response.Cookies.Append("jwtToken", tokenString, new CookieOptions
                    {
                        HttpOnly = true, // ป้องกันการเข้าถึง cookie ด้วย JavaScript
                        Secure = false,   // ส่ง cookie ผ่าน HTTPS เท่านั้น
                        SameSite = SameSiteMode.Lax, // ป้องกัน CSRF
                        Expires = DateTimeOffset.UtcNow.AddMinutes(30) // อายุของ cookie
                    });

                    if (reader.Read())
                    {
                        var user = new
                        {
                            username = reader.GetString("username"),
                            address = reader.GetString("address")
                        };

                        return Results.Ok(user); // Return the first result
                    }
                    else{
                        return Results.NotFound();
                    }
                }

            }
        }
        return Results.NotFound(); // In case no user is found
    }

}

