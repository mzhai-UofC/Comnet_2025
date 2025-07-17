using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace API.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IConnectionMultiplexer redis, ILogger<HealthController> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    [HttpGet("redis")]
    public async Task<IActionResult> CheckRedis()
    {
        try
        {
            var db = _redis.GetDatabase();
            var pingResult = await db.PingAsync();
            
            // 执行一些基本的 Redis 操作
            var testKey = "health:check:" + DateTime.UtcNow.Ticks;
            await db.StringSetAsync(testKey, "alive", TimeSpan.FromMinutes(1));
            var value = await db.StringGetAsync(testKey);
            await db.KeyDeleteAsync(testKey);
            
            _logger.LogInformation("Redis health check successful - Ping: {PingTime}ms", pingResult.TotalMilliseconds);
            
            return Ok(new { 
                status = "healthy", 
                timestamp = DateTime.UtcNow,
                pingTime = pingResult.TotalMilliseconds,
                testOperation = "success"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            return StatusCode(500, new { 
                status = "unhealthy", 
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("full")]
    public async Task<IActionResult> CheckFull()
    {
        var results = new Dictionary<string, object>();
        
        // 检查 Redis
        try
        {
            var db = _redis.GetDatabase();
            var pingResult = await db.PingAsync();
            results["redis"] = new { status = "healthy", pingTime = pingResult.TotalMilliseconds };
        }
        catch (Exception ex)
        {
            results["redis"] = new { status = "unhealthy", error = ex.Message };
        }
        
        results["timestamp"] = DateTime.UtcNow;
        results["server"] = Environment.MachineName;
        
        return Ok(results);
    }
}