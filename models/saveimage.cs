// using System.Dynamic;
// using System.Text.Json;
// using Google.Protobuf.WellKnownTypes;
// using MySql.Data.MySqlClient;
// using Mysqlx.Datatypes;
// using System.Security.Cryptography;
// using System.IO;
// using MongoDBService;
// using MongoDB.Bson;
// using System.Text;
// using System.IO.Compression;
// using System.Data;
// using AESEnCAndDeC;


// namespace models.ManageFile;
// public class ManageFile
// {
//     private static readonly FileService _fileService = new FileService();
//     private static readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
//     public static async Task SaveImageMetadataToDatabase(ExpandoObject body, MySqlConnection connection, HttpContext context)
//     {
//         dynamic data = body;
//         var fileElement = (JsonElement)data.file;
//         var fileData = JsonSerializer.Deserialize<FileData>(fileElement.ToString()!);
//         var fileContent = Convert.FromBase64String(fileData.content);
//         string fileBytes = System.Text.Encoding.UTF8.GetString(fileContent);
//         // Console.WriteLine($" file = {fileContent.ToString()}");
//         byte[] imageBytes = [];
//         byte[] randomBytes = new byte[25];
//         RandomNumberGenerator.Fill(randomBytes);
//         DateTime currentDateTime = DateTime.Now;
//         // string img_id = BitConverter.ToString(randomBytes).Replace("-", "");
//         string imgId = data.imgId;
//         var fileName = currentDateTime.ToString().Replace("/", "").Replace(":", "").Replace(" ", "") + fileData.name;
//         byte[] myKey = Encoding.UTF8.GetBytes("my_secret_key_123gbasdfe1avdfdse");  // Key ความยาว 16, 24 หรือ 32 bytes ตามที่กำหนด
//         byte[] myIV = Encoding.UTF8.GetBytes("my_initializatio");  // IV ความยาว 16 bytes

//         // บันทึกไฟล์ลงใน File System
//         using (Aes myAes = Aes.Create())
//         {
//             byte[] encrypted = AESEncryption.EncryptStringToBytes_Aes(fileContent, myKey, myIV);
//             // await MongoDBConnection.InsertData(imgId, encrypted);
//             imageBytes = encrypted;
//         }
//         // var test = _fileService.SaveImageToFileSystem(fileName, fileContent);
//         string sql = "INSERT INTO images (img_id,file_name, file_data, UploadedAt) VALUES (@img_id,@file_name, @file_data,@UploadedAt)";
//         using (MySqlCommand command = new MySqlCommand(sql, connection))
//         {
//             command.Parameters.AddWithValue("@img_id", imgId);
//             command.Parameters.AddWithValue("@file_name", fileName);
//             command.Parameters.AddWithValue("@file_data", imageBytes);
//             command.Parameters.AddWithValue("@UploadedAt", currentDateTime);
//             command.ExecuteNonQuery();
//         }
//         // await MongoDBConnection.DeleteData(img_id);
//         // หากเกิดข้อผิดพลาดใน SQL
//         // ลบไฟล์ออกจาก File System เพื่อป้องกันไฟล์ตกค้าง
//         // _fileService.DeleteImageFromFileSystem(fileName, _uploadPath);

//         // context.Response.StatusCode = 500; // ส่งสถานะ 500 กลับไป
//         // context.Response.WriteAsync($"Error saving data to database: {ex.Message}");

//     }


//     public static async Task<IResult> FindImageFromDataBase(ExpandoObject body, MySqlConnection connection, HttpContext context)
//     {
//         dynamic data = body;
//         var imgId = data.imgId;
//         byte[] imageData = [];
//         byte[] myKey = Encoding.UTF8.GetBytes("my_secret_key_123gbasdfe1avdfdse");  // Key ความยาว 16, 24 หรือ 32 bytes ตามที่กำหนด
//         byte[] myIV = Encoding.UTF8.GetBytes("my_initializatio");  // IV ความยาว 16 bytes
//         string sql = "SELECT * FROM images WHERE img_id = @img_id";
//         try
//         {
//             using (MySqlCommand command = new MySqlCommand(sql, connection))
//             {
//                 command.Parameters.AddWithValue("@img_id", imgId);
//                 using (MySqlDataReader reader = command.ExecuteReader())
//                 {
//                     if (reader.Read())
//                     {
//                         var image = new
//                         {
//                             fileName = reader.GetString("file_name"),
//                             fileData = GetBlobData(reader, "file_data"),
//                         };
//                         using (Aes myAes = Aes.Create())
//                         {
//                             byte[] decrypted = AESDecryption.DecryptStringFromBytes_Aes(image.fileData, myKey, myIV);
//                             // await MongoDBConnection.InsertData(imgId, encrypted);
//                             imageData = decrypted;
//                         }
//                     var dataToSend = new {
//                         imageData = imageData,
//                     };
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
//             Console.WriteLine(ex);
//             return Results.BadRequest();
//         }
//         // try
//         // {

//         //     using (Aes myAes = Aes.Create())
//         //     {
//         //         List<BsonDocument> ressult = await MongoDBConnection.FindData(img_id);
//         //         foreach (var documents in ressult)
//         //         {
//         //             byte[] dataFromDB = documents["data"].AsByteArray;
//         //             byte[] roundtrip = DecryptStringFromBytes_Aes(dataFromDB, myKey, myIV);
//         //             dataToreturn = roundtrip;
//         //         }
//         //         return dataToreturn;
//         //     }
//         // }
//         // catch (Exception ex)
//         // {
//         //     Console.WriteLine(ex.Message);
//         //     return dataToreturn;
//         // }
//     }


//     public async Task<IResult> DeleteImageMetadataFromDatabase(ExpandoObject body, MySqlConnection connection, HttpContext context)
//     {
//         dynamic data = body;
//         string img_id = data.img_id();
//         string sql = "DELETE FROM Images WHERE img_id = @img_id";
//         try
//         {
//             using (MySqlCommand command = new MySqlCommand(sql, connection))
//             {

//                 command.Parameters.AddWithValue("@img_id", img_id);
//                 command.ExecuteNonQuery();
//             }
//             // await MongoDBConnection.DeleteData(img_id);
//             return Results.Ok(data);
//             // _fileService.DeleteImageFromFileSystem(img_id, _uploadPath);
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine(ex.Message);
//             return Results.StatusCode(500);
//         }

//     }


//     private static byte[] GetBlobData(MySqlDataReader reader, string columnName)
//     {
//         // ตรวจสอบขนาดข้อมูลในคอลัมน์
//         long length = reader.GetBytes(reader.GetOrdinal(columnName), 0, null, 0, 0);
//         byte[] buffer = new byte[length];
//         // อ่านข้อมูล binary จากคอลัมน์
//         reader.GetBytes(reader.GetOrdinal(columnName), 0, buffer, 0, (int)length);
//         return buffer;
//     }

// }


