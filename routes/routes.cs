
// using System.Dynamic;
// using System.Text.Json;
// using Microsoft.AspNetCore.Authorization;
// using models.User;
// using models.ManageFile;
// using MySql.Data.MySqlClient;
// using models.Product;
// using Microsoft.Extensions.Configuration;
// namespace Routes;



// public static class RouteConfig
// {
//     public static void ConfigureRoutes(this WebApplication app)
//     {
//         var Authroutes = new Dictionary<string, Func<ExpandoObject, MySqlConnection, HttpContext, Task<IResult>>>
//         {
//         };
//         var sellerAuthen = new Dictionary<string, Func<ExpandoObject, MySqlConnection, HttpContext, Task<IResult>>>{
//             {"/insert-product",Product.InsertProduct},
//             {"/getProductByUserId",Product.GetProductByUserId},
//         };
//         // var noAuthRoutesMutiModel = new Dictionary<string, List<Func<ExpandoObject, MySqlConnection, HttpContext, Task<(IResult Result, object Data)>>>>{
//         //         {"/findProduct",[Product.FindProduct,ManageFile.FindImageFromDataBase]},
//         //          {"/getProducts",[Product.GetProduct]},
//         //          {"/insert-product",[Product.InsertProduct]},
//         // }
//      var noAuthRoute = new Dictionary<string, Func<ExpandoObject, MySqlConnection, HttpContext, Task<IResult>>>
//         {
//             // { "/login", User.Login },
//              { "/checkLogin", User.CheckLogin },
//              {"/getProducts",Product.GetProduct},
//             {"/findProduct",Product.FindProduct},
//             {"/findImage",ManageFile.FindImageFromDataBase},
//             // Add more routes here
//         };

//         string connectionString = "Server=192.168.1.53;Database=shoping;User ID=root;Password=;";
//         foreach (var route in Authroutes)
//         {
//             app.MapPost(route.Key, [Authorize] async Task<IResult> (HttpContext context) =>
//             {
//                 using MySqlConnection connection = new MySqlConnection(connectionString);
//                 try
//                 {
//                     connection.Open();

//                     // // อ่านข้อมูลจาก form ทั้งหมด
//                     // var form = await context.Request.ReadFormAsync();

//                     // // สร้าง Dictionary เก็บข้อมูลที่ถูกส่งมา
//                     // var formData = new Dictionary<string, string>();

//                     // foreach (var key in form.Keys)
//                     // {
//                     //     formData[key] = form[key]; // เก็บ key-value แต่ละตัวลงใน Dictionary
//                     // }
//                     // Console.WriteLine("formData", formData);
//                     // // คุณสามารถส่งข้อมูล formData (ซึ่งเป็น Dictionary) ไปใช้งานต่อ
//                     // var result = route.Value(formData, connection,context); // ส่ง Dictionary ไปยังฟังก์ชัน

//                     // return result; // ส่งผลลัพธ์กลับไปยัง client

//                     // else
//                     // {

//                     using var reader = new StreamReader(context.Request.Body);
//                     var body = await reader.ReadToEndAsync();
//                     var jsonBody = JsonSerializer.Deserialize<ExpandoObject>(body);

//                     // Directly use the connection here, while it is still open and valid
//                     var result = route.Value(jsonBody, connection, context); // Pass the open connection

//                     return await result; // Return the result to the client
//                     // }

//                 }
//                 catch (Exception ex)
//                 {
//                     // Log and return an error response if something goes wrong
//                     Console.WriteLine($"Exception occurred: {ex.Message}");
//                     return Results.Problem("An error occurred while processing your request.");
//                 }

//             });
//         }

//         foreach (var route in noAuthRoute)
//         {
//             app.MapPost(route.Key, async Task<IResult> (HttpContext context) =>
//             {
//                 using MySqlConnection connection = new MySqlConnection(connectionString);
//                 try
//                 {
//                     connection.Open();
//                     // // อ่านข้อมูลจาก form ทั้งหมด
//                     // var form = await context.Request.ReadFormAsync();

//                     // // สร้าง Dictionary เก็บข้อมูลที่ถูกส่งมา
//                     // var formData = new Dictionary<string, string>();

//                     // foreach (var key in form.Keys)
//                     // {
//                     //     formData[key] = form[key]; // เก็บ key-value แต่ละตัวลงใน Dictionary
//                     // }
//                     // Console.WriteLine("formData", formData);
//                     // // คุณสามารถส่งข้อมูล formData (ซึ่งเป็น Dictionary) ไปใช้งานต่อ
//                     // var result = route.Value(formData, connection,context); // ส่ง Dictionary ไปยังฟังก์ชัน

//                     // return result; // ส่งผลลัพธ์กลับไปยัง client

//                     // else
//                     // {
//                     // Console.WriteLine(context.Request.ContentType);
//                     // var form = await context.Request.ReadFormAsync();
//                     // ดึงข้อมูลไฟล์
//                     // var file = form.Files["file"];
//                     // if (file == null)
//                     // {
//                     //     return Results.BadRequest("No file uploaded.");
//                     // }

//                     // ดึง metadata จาก form (ถ้ามี)
//                     // ExpandoObject jsonBody = new ExpandoObject();
//                     // if (form.TryGetValue("metadata", out var metadataValue))
//                     // {
//                     //     jsonBody = JsonSerializer.Deserialize<ExpandoObject>(metadataValue);
//                     // }
//                     // Console.WriteLine($"File: {file.FileName}");
//                     // Console.WriteLine($"Metadata: {JsonSerializer.Serialize(jsonBody)}");
//                     using var reader = new StreamReader(context.Request.Body);
//                     var body = await reader.ReadToEndAsync();
//                     var jsonBody = JsonSerializer.Deserialize<ExpandoObject>(body);

//                     // Directly use the connection here, while it is still open and valid
//                     var result = route.Value(jsonBody, connection, context); // Pass the open connection

//                     return  await result; // Return the result to the client
//                     // }

//                 }
//                 catch (Exception ex)
//                 {
//                     // Log and return an error response if something goes wrong
//                     Console.WriteLine($"Exception occurred: {ex.Message}");
//                     return Results.Problem("An error occurred while processing your request.");
//                 }

//             });
//         }
//         foreach (var route in sellerAuthen)
//         {
//             app.MapPost(route.Key, [Authorize(Roles = "Seller")] async Task<IResult> (HttpContext context) =>
//             {
//                 using MySqlConnection connection = new MySqlConnection(connectionString);
//                 try
//                 {
//                     connection.Open();
//                     using var reader = new StreamReader(context.Request.Body);
//                     var body = await reader.ReadToEndAsync();
//                     var jsonBody = JsonSerializer.Deserialize<ExpandoObject>(body);
//                     var result = route.Value(jsonBody, connection, context); // Pass the open connection
//                     return await result; // Return the result to the client
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine($"Exception occurred: {ex.Message}");
//                     return Results.Problem("An error occurred while processing your request.");
//                 }

//             });
//         }
//         // foreach (var route in noAuthRoutesMutiModel)
//         // {
//         //     app.MapPost(route.Key, async Task<IResult> (HttpContext context) =>
//         //     {
//         //         using MySqlConnection connection = new MySqlConnection(connectionString);
//         //         try
//         //         {
//         //             connection.Open();
//         //             // // อ่านข้อมูลจาก form ทั้งหมด
//         //             // var form = await context.Request.ReadFormAsync();

//         //             // // สร้าง Dictionary เก็บข้อมูลที่ถูกส่งมา
//         //             // var formData = new Dictionary<string, string>();

//         //             // foreach (var key in form.Keys)
//         //             // {
//         //             //     formData[key] = form[key]; // เก็บ key-value แต่ละตัวลงใน Dictionary
//         //             // }
//         //             // Console.WriteLine("formData", formData);
//         //             // // คุณสามารถส่งข้อมูล formData (ซึ่งเป็น Dictionary) ไปใช้งานต่อ
//         //             // var result = route.Value(formData, connection,context); // ส่ง Dictionary ไปยังฟังก์ชัน

//         //             // return result; // ส่งผลลัพธ์กลับไปยัง client

//         //             // else
//         //             // {
//         //             // Console.WriteLine(context.Request.ContentType);
//         //             // var form = await context.Request.ReadFormAsync();
//         //             // ดึงข้อมูลไฟล์
//         //             // var file = form.Files["file"];
//         //             // if (file == null)
//         //             // {
//         //             //     return Results.BadRequest("No file uploaded.");
//         //             // }

//         //             // ดึง metadata จาก form (ถ้ามี)
//         //             // ExpandoObject jsonBody = new ExpandoObject();
//         //             // if (form.TryGetValue("metadata", out var metadataValue))
//         //             // {
//         //             //     jsonBody = JsonSerializer.Deserialize<ExpandoObject>(metadataValue);
//         //             // }
//         //             // Console.WriteLine($"File: {file.FileName}");
//         //             // Console.WriteLine($"Metadata: {JsonSerializer.Serialize(jsonBody)}");
//         //             using var reader = new StreamReader(context.Request.Body);
//         //             var body = await reader.ReadToEndAsync();
//         //             var jsonBody = JsonSerializer.Deserialize<ExpandoObject>(body);

//         //             // Directly use the connection here, while it is still open and valid
//         //             var result = ControllerConfig.Controller(jsonBody, connection, context,route.Value); // Pass the open connection

//         //             return  await result; // Return the result to the client
//         //             // }

//         //         }
//         //         catch (Exception ex)
//         //         {
//         //             // Log and return an error response if something goes wrong
//         //             Console.WriteLine($"Exception occurred: {ex.Message}");
//         //             return Results.Problem("An error occurred while processing your request.");
//         //         }

//         //     });
//         // }


//     }

// }


