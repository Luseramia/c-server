using System.Dynamic;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using models.User;
using MySql.Data.MySqlClient;
namespace Routes;



public static class RouteConfig
{
    public static void ConfigureRoutes(this WebApplication app)
    {
        var routes = new Dictionary<string, Func<ExpandoObject, MySqlConnection,HttpContext,IResult>>
        {
            { "/post1", User.Login },
            // Add more routes here
        };
        string connectionString = "Server=192.168.1.53;Database=nrru;User ID=root;Password=fromis9;";
        foreach (var route in routes)
        {
            app.MapPost(route.Key,async (HttpContext context) =>
            {
                using MySqlConnection connection = new MySqlConnection(connectionString);
                try
                {
                    connection.Open();
                 
                        // // อ่านข้อมูลจาก form ทั้งหมด
                        // var form = await context.Request.ReadFormAsync();

                        // // สร้าง Dictionary เก็บข้อมูลที่ถูกส่งมา
                        // var formData = new Dictionary<string, string>();

                        // foreach (var key in form.Keys)
                        // {
                        //     formData[key] = form[key]; // เก็บ key-value แต่ละตัวลงใน Dictionary
                        // }
                        // Console.WriteLine("formData", formData);
                        // // คุณสามารถส่งข้อมูล formData (ซึ่งเป็น Dictionary) ไปใช้งานต่อ
                        // var result = route.Value(formData, connection,context); // ส่ง Dictionary ไปยังฟังก์ชัน

                        // return result; // ส่งผลลัพธ์กลับไปยัง client
                    
                    // else
                    // {

                        using var reader = new StreamReader(context.Request.Body);
                        var body = await reader.ReadToEndAsync();
                        Console.WriteLine(body);
                        var jsonBody = JsonSerializer.Deserialize<ExpandoObject>(body);
          
                        // Directly use the connection here, while it is still open and valid
                        var result = route.Value(jsonBody, connection,context); // Pass the open connection

                        return result; // Return the result to the client
                    // }

                }
                catch (Exception ex)
                {
                    // Log and return an error response if something goes wrong
                    Console.WriteLine($"Exception occurred: {ex.Message}");
                    return Results.Problem("An error occurred while processing your request.");
                }

            });
        }




    }

}


public class Test
{
    public string endpoint { get; set; }
    public Func<string, MySqlConnection, IResult> model { get; set; }
    public Test()
    {

    }
}

