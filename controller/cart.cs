using System.Data;
using System.Dynamic;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Controllers
{
    [ApiController]
    [Route("cart")]
    public class CartController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CartController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet("getItemInCart")]
        public async Task<IActionResult> GetItemInCartByUserId()
        {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        try
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using MySqlConnection connection = new MySqlConnection(connectionString);
            string sql = "SELECT productId, SUM(quantity) AS quantity FROM cart WHERE userId = @userId GROUP BY productId";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@userId", userId);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            var cartItems = new List<dynamic>();
            // if (reader.HasRows && await reader.ReadAsync()){

            while (await reader.ReadAsync())
            {
                dynamic cartItem = new ExpandoObject();
                            cartItem.productId = reader.GetString("productId");
                            cartItem.quantity = reader.GetInt32("quantity");
                // เพิ่ม property อื่นๆ ตามต้องการ
                
                cartItems.Add(cartItem);
            }
            return Ok(cartItems);
        }
        catch (Exception ex){
            Console.WriteLine(ex.ToString());
            return BadRequest("Failed to insert item to  cart");
        }
        }
    
        [Authorize]
        [HttpPost("insertCartItemByUserId")]
        public async Task<IActionResult> InsertCartItemByUserId([FromBody] CartItem model)
        {
        
        byte[] randomBytes = new byte[25];
        RandomNumberGenerator.Fill(randomBytes);
        string cartId = BitConverter.ToString(randomBytes).Replace("-", "");
        DateTime currentDateTime = DateTime.Now;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        try
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using MySqlConnection connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            string sql = "INSERT INTO cart (cartId,productId,userId,quantity,updateAt) VALUES (@cartId,@productId,@userId,@quantity,@updateAt)";
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@cartId", cartId);
                command.Parameters.AddWithValue("@productId", model.productId);
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@quantity", model.quantity);
                command.Parameters.AddWithValue("@updateAt", currentDateTime);
                command.ExecuteNonQuery();
            }
            return Ok();
        }
        catch (Exception ex){
            Console.WriteLine(ex.ToString());
            return BadRequest("Failed to insert item to  cart");
        }
        }
    }
}

public class CartItem{
    public string productId{get;set;}
    public int  quantity{get;set;}
    
}