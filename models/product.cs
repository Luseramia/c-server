
using System.Dynamic;
using System.Text.Json;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using MongoDBService;
using System.Text;
using models.ManageFile;

namespace models.Product;
public class Product
{
    public static async Task<IResult> InsertProduct(ExpandoObject body, MySqlConnection connection, HttpContext context)
    {
        dynamic data = body;
        var productName = data.productName;
        var productDescription = data.productDescription;
        var productPrice = data.productPrice;

        byte[] randomBytes = new byte[25];
        RandomNumberGenerator.Fill(randomBytes);
        string productId = BitConverter.ToString(randomBytes).Replace("-", "");
        DateTime currentDateTime = DateTime.Now;
        byte[] randomBytesForImgId = new byte[25];
        RandomNumberGenerator.Fill(randomBytesForImgId);
        string imgId = BitConverter.ToString(randomBytes).Replace("-", "");
        try
        {
            var bodyDict = body as IDictionary<string, object>;
            if (bodyDict != null)
            {
                bodyDict["imgId"] = imgId;
            }
            // หากบันทึกไฟล์สำเร็จ บันทึก Metadata ในฐานข้อมูล
            string sql = "INSERT INTO product (product_id,product_name, product_description,product_price, img_id,uploadedAt) VALUES (@product_id,@product_name, @product_description,@product_price,@img_id,@uploadedAt)";
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@product_id", productId);
                command.Parameters.AddWithValue("@product_name", productName);
                command.Parameters.AddWithValue("@product_description", productDescription);
                command.Parameters.AddWithValue("@product_price", productPrice);
                command.Parameters.AddWithValue("@img_id", imgId);
                command.Parameters.AddWithValue("@uploadedAt", currentDateTime);
                command.ExecuteNonQuery();
            }
            await ManageFile.ManageFile.SaveImageMetadataToDatabase(body, connection, context);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            // หากเกิดข้อผิดพลาดใน SQL
            // ลบไฟล์ออกจาก File System เพื่อป้องกันไฟล์ตกค้าง
            // _fileService.DeleteImageFromFileSystem(fileName, _uploadPath);
            Console.WriteLine($"Error saving data to database product: {ex.Message}");
            // context.Response.StatusCode = 500; // ส่งสถานะ 500 กลับไป
            // context.Response.WriteAsync($"Error saving data to database: {ex.Message}");
            return Results.StatusCode(500); // หยุดการทำงานของฟังก์ชัน
        }
    }
}
