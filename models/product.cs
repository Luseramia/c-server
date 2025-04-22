
// using System.Dynamic;
// using System.Text.Json;
// using MySql.Data.MySqlClient;
// using System.Security.Cryptography;
// using MongoDBService;
// using System.Text;
// using models.ManageFile;
// using System.Data;
// using AESEnCAndDeC;
// using System.Security.Claims;
// namespace models.Product;
// public class Product
// {
//     public static async Task<IResult> InsertProduct(ExpandoObject body, MySqlConnection connection, HttpContext context)
//     {
//         dynamic data = body;
//         var productName = data.productName;
//         var productDescription = data.productDescription;
//         var productPrice = data.productPrice;
//         var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//         byte[] randomBytes = new byte[25];
//         RandomNumberGenerator.Fill(randomBytes);
//         string productId = BitConverter.ToString(randomBytes).Replace("-", "");
//         DateTime currentDateTime = DateTime.Now;
//         byte[] randomBytesForImgId = new byte[25];
//         RandomNumberGenerator.Fill(randomBytesForImgId);
//         string imgId = BitConverter.ToString(randomBytes).Replace("-", "");
//         try
//         {
//             var bodyDict = body as IDictionary<string, object>;
//             if (bodyDict != null)
//             {
//                 bodyDict["imgId"] = imgId;
//             }
//             // หากบันทึกไฟล์สำเร็จ บันทึก Metadata ในฐานข้อมูล
//             string sql = "INSERT INTO product (product_id,product_name, product_description,product_price, img_id,uploadedAt,userId) VALUES (@product_id,@product_name, @product_description,@product_price,@img_id,@uploadedAt,@userId)";
//             using (MySqlCommand command = new MySqlCommand(sql, connection))
//             {
//                 command.Parameters.AddWithValue("@product_id", productId);
//                 command.Parameters.AddWithValue("@product_name", productName);
//                 command.Parameters.AddWithValue("@product_description", productDescription);
//                 command.Parameters.AddWithValue("@product_price", productPrice);
//                 command.Parameters.AddWithValue("@img_id", imgId);
//                 command.Parameters.AddWithValue("@uploadedAt", currentDateTime);
//                 command.Parameters.AddWithValue("@userId", userId);
//                 command.ExecuteNonQuery();
//             }
//             await ManageFile.ManageFile.SaveImageMetadataToDatabase(body, connection, context);

//             return Results.Ok();
//         }
//         catch (Exception ex)
//         {
//             // หากเกิดข้อผิดพลาดใน SQL
//             // ลบไฟล์ออกจาก File System เพื่อป้องกันไฟล์ตกค้าง
//             // _fileService.DeleteImageFromFileSystem(fileName, _uploadPath);
//             Console.WriteLine($"Error saving data to database product: {ex.Message}");
//             // context.Response.StatusCode = 500; // ส่งสถานะ 500 กลับไป
//             // context.Response.WriteAsync($"Error saving data to database: {ex.Message}");
//             return Results.StatusCode(500); // หยุดการทำงานของฟังก์ชัน
//         }
//     }
//     public static async Task<IResult> FindProduct(ExpandoObject body, MySqlConnection connection, HttpContext context)
//     {
//         dynamic data = body;
//         var productId = data.productId;
//         try
//         {
//             string sql = "SELECT * FROM product WHERE product_id = @product_id";
//             using (MySqlCommand command = new MySqlCommand(sql, connection))
//             {
//                 command.Parameters.AddWithValue("@product_id", productId);
//                 using (MySqlDataReader reader = command.ExecuteReader())
//                 {
//                     if (reader.Read())
//                     {
//                         var product = new
//                         {
//                             productId = reader.GetString("product_id"),
//                             productName = reader.GetString("product_name"),
//                             productDescription = reader.GetString("product_description"),
//                             productPrice = reader.GetInt32("product_price"),
//                             imgId = reader.GetString("img_id")
//                         };
//                         // var bodyDict = body as IDictionary<string, object>;
//                         //  if (bodyDict != null)
//                         //      {
//                         //         bodyDict["img_id"] = product.imgId;
//                         //       }
//                     //    byte[] image;
//                         // using (var newConnection = new MySqlConnection(connection.ConnectionString))
//                         // {
//                         //     await newConnection.OpenAsync();
//                         //     image = await ManageFile.ManageFile.FindImageFromDataBase(body, newConnection, context);
//                         // }
//                         ProductData dataToSend = new ProductData
//                         {
//                             productId = product.productId,
//                             productName = product.productName,
//                             productDescription = product.productDescription,
//                             productPrice = product.productPrice,
//                             imgId = product.imgId,
//                         };
//                         return Results.Ok(dataToSend); // Return the first result
//                     }
//                     else
//                     {
//                         return Results.NotFound();
//                     }
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             // หากเกิดข้อผิดพลาดใน SQL
//             // ลบไฟล์ออกจาก File System เพื่อป้องกันไฟล์ตกค้าง
//             // _fileService.DeleteImageFromFileSystem(fileName, _uploadPath);
//             Console.WriteLine($"Error saving data to database product: {ex.Message}");
//             // context.Response.StatusCode = 500; // ส่งสถานะ 500 กลับไป
//             // context.Response.WriteAsync($"Error saving data to database: {ex.Message}");
//             return Results.StatusCode(500); // หยุดการทำงานของฟังก์ชัน
//         }
//     }


//     public static async Task<IResult> GetProduct(ExpandoObject body, MySqlConnection connection, HttpContext context)
//     {
//         dynamic data = body;
//         try
//         {
//             string sql = "SELECT * FROM product INNER JOIN images ON product.img_id = images.img_id";
//             using (MySqlCommand command = new MySqlCommand(sql, connection))
//             {
//                 using (MySqlDataReader reader = command.ExecuteReader())
//                 {
//                     var products = new List<object>();
//                     while (reader.Read())
//                     {
//                         var product = new
//                         {
//                             productId = reader.GetString("product_id"),
//                             productName = reader.GetString("product_name"),
//                             productDescription = reader.GetString("product_description"),
//                             productPrice = reader.GetInt32("product_price"),
//                             productImage = GetBlobData(reader, "file_data"),
//                         };
//                         products.Add(product);
//                     }
//                     // foreach(var product in products){
//                     //     Console.WriteLine(product);
//                     // }
//                     return Results.Ok(products);
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             // หากเกิดข้อผิดพลาดใน SQL
//             // ลบไฟล์ออกจาก File System เพื่อป้องกันไฟล์ตกค้าง
//             // _fileService.DeleteImageFromFileSystem(fileName, _uploadPath);
//             Console.WriteLine($"Error get data to database product: {ex.Message}");
//             // context.Response.StatusCode = 500; // ส่งสถานะ 500 กลับไป
//             // context.Response.WriteAsync($"Error saving data to database: {ex.Message}");
//             return Results.StatusCode(500); // หยุดการทำงานของฟังก์ชัน
//         }
//     }


//         public static async Task<IResult> GetProductByUserId(ExpandoObject body, MySqlConnection connection, HttpContext context)
//     {
//         dynamic data = body;
//         var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//         try
//         {
//             string sql = "SELECT * FROM product  INNER JOIN images ON product.img_id = images.img_id WHERE userId = @userId";
//             using (MySqlCommand command = new MySqlCommand(sql, connection))
//             {
//                 command.Parameters.AddWithValue("@userId", userId);
//                 using (MySqlDataReader reader = command.ExecuteReader())
//                 {
//                     var products = new List<object>();
//                     while (reader.Read())
//                     {
//                         var product = new
//                         {
//                             productId = reader.GetString("product_id"),
//                             productName = reader.GetString("product_name"),
//                             productDescription = reader.GetString("product_description"),
//                             productPrice = reader.GetInt32("product_price"),
//                             productImage = GetBlobData(reader, "file_data"),
//                         };
//                         products.Add(product);
//                     }
//                     // foreach(var product in products){
//                     //     Console.WriteLine(product);
//                     // }
//                     return Results.Ok(products);
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             // หากเกิดข้อผิดพลาดใน SQL
//             // ลบไฟล์ออกจาก File System เพื่อป้องกันไฟล์ตกค้าง
//             // _fileService.DeleteImageFromFileSystem(fileName, _uploadPath);
//             Console.WriteLine($"Error get data to database product: {ex.Message}");
//             // context.Response.StatusCode = 500; // ส่งสถานะ 500 กลับไป
//             // context.Response.WriteAsync($"Error saving data to database: {ex.Message}");
//             return Results.StatusCode(500); // หยุดการทำงานของฟังก์ชัน
//         }
//     }

//     private static byte[] GetBlobData(MySqlDataReader reader, string columnName)
//     {
//         byte[] myKey = Encoding.UTF8.GetBytes("my_secret_key_123gbasdfe1avdfdse");  // Key ความยาว 16, 24 หรือ 32 bytes ตามที่กำหนด
//         byte[] myIV = Encoding.UTF8.GetBytes("my_initializatio");  // IV ความยาว 16 bytes
//         // ตรวจสอบขนาดข้อมูลในคอลัมน์
//         long length = reader.GetBytes(reader.GetOrdinal(columnName), 0, null, 0, 0);
//         byte[] buffer = new byte[length];
//         // อ่านข้อมูล binary จากคอลัมน์
//         reader.GetBytes(reader.GetOrdinal(columnName), 0, buffer, 0, (int)length);
//         using (Aes myAes = Aes.Create())
//         {
//             byte[] decrypted = AESDecryption.DecryptStringFromBytes_Aes(buffer, myKey, myIV);
//             // await MongoDBConnection.InsertData(imgId, encrypted);
//             buffer = decrypted;
//         }
//         return buffer;
//     }

    
// }
