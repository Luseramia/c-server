using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RedisServices
{
    public interface ITokenService
    {
        Task<bool> IsTokenValid(string userId, string token);
        Task SaveToken(string userId, string token, TimeSpan expiry);
        Task RevokeToken(string userId, string token);
        Task RevokeAllUserTokens(string userId);
        Task<string> GenerateTokenAsync(string userId, string role);
    }

    public class RedisTokenService : ITokenService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IConfiguration _configuration;

        public RedisTokenService(IConnectionMultiplexer redis, IConfiguration configuration)
        {
            _redis = redis;
            _configuration = configuration;
        }

        public async Task<bool> IsTokenValid(string userId, string token)
        {
            var db = _redis.GetDatabase();
            
            // ตรวจสอบว่า token อยู่ใน valid tokens ของ user นี้หรือไม่
            var isValid = await db.SetContainsAsync($"user_tokens:{userId}", token);
            
            return isValid;
        }

        public async Task SaveToken(string userId, string token, TimeSpan expiry)
        {
            var db = _redis.GetDatabase();
            
            // เก็บ token ในเซ็ตของ user (user_tokens:userId)
            await db.SetAddAsync($"user_tokens:{userId}", token);
            
            // ตั้งเวลาหมดอายุสำหรับเซ็ตของ user (ถ้ายังไม่มี)
            await db.KeyExpireAsync($"user_tokens:{userId}", expiry);
        }

        public async Task RevokeToken(string userId, string token)
        {
            var db = _redis.GetDatabase();
            
            // ลบ token จากเซ็ตของ user
            await db.SetRemoveAsync($"user_tokens:{userId}", token);
        }

        public async Task RevokeAllUserTokens(string userId)
        {
            var db = _redis.GetDatabase();
            
            // ลบเซ็ตของ user ทั้งหมด
            await db.KeyDeleteAsync($"user_tokens:{userId}");
        }

        public Task<string> GenerateTokenAsync(string userId, string role)
        {
            throw new NotImplementedException();
        }
    }
}