using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Text.Json;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using System.Security.Cryptography;
using AESEnCAndDeC;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Controllers.ManageFile;
namespace Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProductController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using MySqlConnection connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = "SELECT * FROM product INNER JOIN images ON product.img_id = images.img_id";
            using var command = new MySqlCommand(sql, connection);

            using var reader = await command.ExecuteReaderAsync();
            var products = new List<dynamic>();

            while (await reader.ReadAsync())
            {
                dynamic product = new ExpandoObject();
                       product.productId = reader.GetString("product_id");
                            product.productName = reader.GetString("product_name");
                            product.productDescription = reader.GetString("product_description");
                            product.productPrice = reader.GetInt32("product_price");
                            product.productImage = GetBlobData((MySqlDataReader)reader, "file_data");
                // เพิ่ม property อื่นๆ ตามต้องการ
                
                products.Add(product);
            }

            return Ok(products);
        }

        // [HttpGet("{id}")]
        // public async Task<IActionResult> GetProductById(string id)
        // {
        //     string connectionString = _configuration.GetConnectionString("DefaultConnection");
        //     using MySqlConnection connection = new MySqlConnection(connectionString);
        //     await connection.OpenAsync();

        //     string sql = "SELECT * FROM products WHERE productId = @productId";
        //     using var command = new MySqlCommand(sql, connection);
        //     command.Parameters.AddWithValue("@productId", id);

        //     using var reader = await command.ExecuteReaderAsync();
        //     if (reader.HasRows && await reader.ReadAsync())
        //     {
        //         dynamic product = new ExpandoObject();
        //         product.productId = reader.GetString("productId");
        //         product.name = reader.GetString("name");
        //         product.price = reader.GetDecimal("price");
        //         // เพิ่ม property อื่นๆ ตามต้องการ

        //         return Ok(product);
        //     }

        //     return NotFound($"Product with ID {id} not found");
        // }

        [HttpPost("getProductByUserId")]
        public async Task<IActionResult> GetProductByUserId()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using MySqlConnection connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string sql = "SELECT * FROM product INNER JOIN images ON product.img_id = images.img_id WHERE userId = @userId";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@userId", userId);
            using var reader = await command.ExecuteReaderAsync();
            var products = new List<dynamic>();

            while (await reader.ReadAsync())
            {
                dynamic product = new ExpandoObject();
                       product.productId = reader.GetString("product_id");
                            product.productName = reader.GetString("product_name");
                            product.productDescription = reader.GetString("product_description");
                            product.productPrice = reader.GetInt32("product_price");
                            product.productImage = GetBlobData((MySqlDataReader)reader, "file_data");
                // เพิ่ม property อื่นๆ ตามต้องการ
                
                products.Add(product);
            }

            return Ok(products);
        }


        [HttpPost("insertProductType")]
        public async Task<IActionResult> InsertProductType([FromBody] ProductType model)
        {
            byte[] randomBytes = new byte[25];
            RandomNumberGenerator.Fill(randomBytes);
            string type_id = BitConverter.ToString(randomBytes).Replace("-", "");
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using MySqlConnection connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            string sql = "INSERT INTO productType (type_id,type_name) VALUES (@type_id,@type_name)";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@type_id", type_id);
            command.Parameters.AddWithValue("@type_name", model.typeName);
            using var reader = await command.ExecuteReaderAsync();
            var products = new List<dynamic>();

            while (await reader.ReadAsync())
            {
                dynamic product = new ExpandoObject();
                       product.productId = reader.GetString("product_id");
                            product.productName = reader.GetString("product_name");
                            product.productDescription = reader.GetString("product_description");
                            product.productPrice = reader.GetInt32("product_price");
                            product.productImage = GetBlobData((MySqlDataReader)reader, "file_data");
                // เพิ่ม property อื่นๆ ตามต้องการ
                
                products.Add(product);
            }

            return Ok(products);
        }
        [HttpPost]
        public async Task<IActionResult> InsertProduct([FromBody] InsertProductModel model)
        {
        
        byte[] randomBytes = new byte[25];
        RandomNumberGenerator.Fill(randomBytes);
        string productId = BitConverter.ToString(randomBytes).Replace("-", "");
        DateTime currentDateTime = DateTime.Now;
        byte[] randomBytesForImgId = new byte[25];
        RandomNumberGenerator.Fill(randomBytesForImgId);
        string imgId = BitConverter.ToString(randomBytes).Replace("-", "");
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        try
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using MySqlConnection connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            string sql = "INSERT INTO product (product_id,product_name, product_description,product_price, img_id,typeId,tag,uploadedAt,userId) VALUES (@product_id,@product_name, @product_description,@product_price,@img_id,@typeId,@tag,@uploadedAt,@userId)";
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@product_id", productId);
                command.Parameters.AddWithValue("@product_name", model.productName);
                command.Parameters.AddWithValue("@product_description", model.productDescription);
                command.Parameters.AddWithValue("@product_price", model.productPrice);
                command.Parameters.AddWithValue("@img_id", imgId);
                command.Parameters.AddWithValue("@typeId", model.typeId);
                command.Parameters.AddWithValue("@tag", model.tag);
                command.Parameters.AddWithValue("@uploadedAt", currentDateTime);
                command.Parameters.AddWithValue("@userId", userId);
                command.ExecuteNonQuery();
            }
            var imageContent = new ImageModel{
                imgId = imgId,
                file = model.file
            };
            try{
            ManageFile.ManageFile.SaveImageMetadataToDatabase(imageContent,connection);
            return Ok();
            }
            catch{
                return BadRequest("Failed to create product");
            }
        }
        catch{
            return BadRequest("Failed to create product");
        }
        }
    public static byte[] GetBlobData(MySqlDataReader reader, string columnName)
    {
        byte[] myKey = Encoding.UTF8.GetBytes("my_secret_key_123gbasdfe1avdfdse");  // Key ความยาว 16, 24 หรือ 32 bytes ตามที่กำหนด
        byte[] myIV = Encoding.UTF8.GetBytes("my_initializatio");  // IV ความยาว 16 bytes
        // ตรวจสอบขนาดข้อมูลในคอลัมน์
        long length = reader.GetBytes(reader.GetOrdinal(columnName), 0, null, 0, 0);
        byte[] buffer = new byte[length];
        // อ่านข้อมูล binary จากคอลัมน์
        reader.GetBytes(reader.GetOrdinal(columnName), 0, buffer, 0, (int)length);
        using (Aes myAes = Aes.Create())
        {
            byte[] decrypted = AESDecryption.DecryptStringFromBytes_Aes(buffer, myKey, myIV);
            // await MongoDBConnection.InsertData(imgId, encrypted);
            buffer = decrypted;
        }
        return buffer;
    }
    }

    public class InsertProductModel
    {
        public string productName { get; set; }
        
        public string productDescription { get; set; }
        public decimal productPrice { get; set; }
        public FileData file {get;set;}
        public string typeId { get; set; }
         public string tag { get; set; }
        
    }
    public class ImageModel{
        public string imgId { get; set; }
        public FileData file {get;set;}
    }

    public class ProductType{
        public string typeName {get;set;}
    }
    
}


