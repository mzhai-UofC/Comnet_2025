using StackExchange.Redis;

namespace API.Services;

public class RedisKeepAliveService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisKeepAliveService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15); // 每15分钟执行一次

    public RedisKeepAliveService(IConnectionMultiplexer redis, ILogger<RedisKeepAliveService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Redis Keep-Alive Service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformRedisOperations();
                _logger.LogInformation("Redis keep-alive operations completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis keep-alive operations failed");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task PerformRedisOperations()
    {
        var db = _redis.GetDatabase();
        
        // 1. Ping 操作
        var pingResult = await db.PingAsync();
        
        // 2. 写入和读取操作
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var key = $"keepalive:{timestamp}";
        
        await db.StringSetAsync(key, $"alive-{timestamp}", TimeSpan.FromMinutes(30));
        var value = await db.StringGetAsync(key);
        
        // 3. 清理旧的 keepalive 键
        await CleanupOldKeepAliveKeys(db);
        
        _logger.LogDebug("Redis operations - Ping: {PingTime}ms, Key: {Key}, Value: {Value}", 
            pingResult.TotalMilliseconds, key, value);
    }

    private async Task CleanupOldKeepAliveKeys(IDatabase db)
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: "keepalive:*").Take(100); // 限制清理数量
            
            var oldTimestamp = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds();
            
            foreach (var key in keys)
            {
                var keyTimestamp = key.ToString().Split(':')[1];
                if (long.TryParse(keyTimestamp, out var ts) && ts < oldTimestamp)
                {
                    await db.KeyDeleteAsync(key);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup old keep-alive keys");
        }
    }
}