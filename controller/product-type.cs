using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Text.Json;
using MySql.Data.MySqlClient;
using RedisServices;
using JwtGen;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
namespace Controllers
{
[ApiController]
[Route("productType")]
    public class ProductTypeController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProductTypeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using MySqlConnection connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = "SELECT * FROM productType";
            using var command = new MySqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            var productTypes = new List<dynamic>();
            while (await reader.ReadAsync())
            {
                dynamic productType = new ExpandoObject();
                productType.typeId = reader.GetString("type_id");
                productType.typeName = reader.GetString("type_name");
                // เพิ่ม property อื่นๆ ตามต้องการ
                
                productTypes.Add(productType);
            }

            return Ok(productTypes);
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
            return Ok();
        }

    }
}
