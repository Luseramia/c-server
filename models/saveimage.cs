using System.Dynamic;
using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using Mysqlx.Datatypes;
using System.Security.Cryptography;
using System.IO;
using MongoDBService;
using MongoDB.Bson;
using System.Text;
using System.IO.Compression;

namespace models.ManageFile;
public class ManageFile
{
    private static readonly FileService _fileService = new FileService();
    private static readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
    public static async Task SaveImageMetadataToDatabase(ExpandoObject body, MySqlConnection connection, HttpContext context)
    {
        dynamic data = body;
        var fileElement = (JsonElement)data.file;
        var fileData = JsonSerializer.Deserialize<FileData>(fileElement.ToString()!);
        var fileContent = Convert.FromBase64String(fileData.content);
        string fileBytes = System.Text.Encoding.UTF8.GetString(fileContent);
        // Console.WriteLine($" file = {fileContent.ToString()}");
        byte[] imageBytes;
        byte[] randomBytes = new byte[25];
        RandomNumberGenerator.Fill(randomBytes);
        DateTime currentDateTime = DateTime.Now;
        // string img_id = BitConverter.ToString(randomBytes).Replace("-", "");
        string imgId = data.imgId;
        var fileName = currentDateTime.ToString().Replace("/", "").Replace(":", "").Replace(" ", "") + fileData.name;
        byte[] myKey = Encoding.UTF8.GetBytes("my_secret_key_123gbasdfe1avdfdse");  // Key ความยาว 16, 24 หรือ 32 bytes ตามที่กำหนด
        byte[] myIV = Encoding.UTF8.GetBytes("my_initializatio");  // IV ความยาว 16 bytes

        // บันทึกไฟล์ลงใน File System
        using (Aes myAes = Aes.Create())
        {
            byte[] encrypted = EncryptStringToBytes_Aes(fileContent, myKey, myIV);
            await MongoDBConnection.InsertData(imgId, encrypted);
            imageBytes = encrypted;
        }

        // var test = _fileService.SaveImageToFileSystem(fileName, fileContent);


        // หากเกิดข้อผิดพลาดในการบันทึกไฟล์
        // context.Response.StatusCode = 500; // ส่งสถานะ 500 กลับไป
        // context.Response.WriteAsync($"Error saving file to file system: {ex.Message}");
        // return Results.StatusCode(500); // หยุดการทำงานของฟังก์ชัน


        // หากบันทึกไฟล์สำเร็จ บันทึก Metadata ในฐานข้อมูล
        string sql = "INSERT INTO Images (img_id,FileName, FileData, UploadedAt) VALUES (@img_id,@FileName, @FileData,@UploadedAt)";
        using (MySqlCommand command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@img_id", imgId);
            command.Parameters.AddWithValue("@FileName", fileName);
            command.Parameters.AddWithValue("@FileData", imageBytes);
            command.Parameters.AddWithValue("@UploadedAt", currentDateTime);
            command.ExecuteNonQuery();
        }
        // await MongoDBConnection.DeleteData(img_id);
        // หากเกิดข้อผิดพลาดใน SQL
        // ลบไฟล์ออกจาก File System เพื่อป้องกันไฟล์ตกค้าง
        // _fileService.DeleteImageFromFileSystem(fileName, _uploadPath);

        // context.Response.StatusCode = 500; // ส่งสถานะ 500 กลับไป
        // context.Response.WriteAsync($"Error saving data to database: {ex.Message}");

    }


    public static async Task<IResult> FindImageFromDataBase(ExpandoObject body, MySqlConnection connection, HttpContext context)
    {
        dynamic data = body;
        var img_id = data.img_id;
        byte[] dataToreturn = [];
        byte[] myKey = Encoding.UTF8.GetBytes("my_secret_key_123gbasdfe1avdfdse");  // Key ความยาว 16, 24 หรือ 32 bytes ตามที่กำหนด
        byte[] myIV = Encoding.UTF8.GetBytes("my_initializatio");  // IV ความยาว 16 bytes
        try
        {

            using (Aes myAes = Aes.Create())
            {
                List<BsonDocument> ressult = await MongoDBConnection.FindData(img_id);
                foreach (var documents in ressult)
                {
                    byte[] dataFromDB = documents["data"].AsByteArray;
                    byte[] roundtrip = DecryptStringFromBytes_Aes(dataFromDB, myKey, myIV);
                    dataToreturn = roundtrip;
                }
                return Results.File(dataToreturn, "image/jpeg");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Results.StatusCode(500);
        }
    }


    public async Task<IResult> DeleteImageMetadataFromDatabase(ExpandoObject body, MySqlConnection connection, HttpContext context)
    {
        dynamic data = body;
        string img_id = data.img_id();
        string sql = "DELETE FROM Images WHERE img_id = @img_id";
        try
        {
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {

                command.Parameters.AddWithValue("@img_id", img_id);
                command.ExecuteNonQuery();
            }
            await MongoDBConnection.DeleteData(img_id);
            return Results.Ok(data);
            // _fileService.DeleteImageFromFileSystem(img_id, _uploadPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Results.StatusCode(500);
        }

    }



    private static byte[] EncryptStringToBytes_Aes(byte[] plainText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException("plainText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");

        byte[] encrypted;

        // Create an Aes object with the specified key and IV
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;
            aesAlg.Padding = PaddingMode.PKCS7;  // Set padding mode for AES

            // Create an encryptor to perform the stream transform
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    // Write all data to the stream
                    csEncrypt.Write(plainText, 0, plainText.Length);
                }

                encrypted = msEncrypt.ToArray();  // Get the encrypted data
            }
        }

        // Return the encrypted bytes from the memory stream
        return encrypted;
    }


    private static byte[] DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException("cipherText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");

        byte[] plaintext;

        // Create an Aes object with the specified key and IV
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;
            aesAlg.Padding = PaddingMode.PKCS7;  // Set padding mode for AES

            // Create a decryptor to perform the stream transform
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    // Use MemoryStream to hold the decrypted data
                    using (var msOutput = new MemoryStream())
                    {
                        csDecrypt.CopyTo(msOutput);  // Copy decrypted data to MemoryStream
                        plaintext = msOutput.ToArray();  // Return the decrypted byte array
                    }
                }
            }
        }

        return plaintext;  // Return the decrypted data
    }
}