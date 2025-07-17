using Core.Entities;
using Core.Interfaces;

namespace API.Services;

public class CartKeepAliveService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CartKeepAliveService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(2); // 每2小时执行一次

    public CartKeepAliveService(IServiceProvider serviceProvider, ILogger<CartKeepAliveService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cart Keep-Alive Service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();
                
                await PerformCartOperations(cartService);
                _logger.LogInformation("Cart keep-alive operations completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cart keep-alive operations failed");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task PerformCartOperations(ICartService cartService)
    {
        var healthCartId = $"health-check-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        
        try
        {
            // 创建健康检查购物车
            var healthCart = new ShoppingCart
            {
                Id = healthCartId,
                Items = new List<CartItem>
                {
                    new()
                    {
                        ProductId = 1,
                        ProductName = "Health Check Item",
                        Price = 1.00m,
                        Quantity = 1,
                        Brand = "System",
                        Type = "Health Check",
                        PictureUrl = ""
                    }
                }
            };
            
            // 执行购物车操作
            await cartService.SetCartAsync(healthCart);
            var retrievedCart = await cartService.GetCartAsync(healthCartId);
            
            if (retrievedCart != null)
            {
                _logger.LogDebug("Health cart retrieved successfully with {ItemCount} items", retrievedCart.Items.Count);
            }
            
            // 清理健康检查购物车
            await cartService.DeleteCartAsync(healthCartId);
            
            _logger.LogDebug("Cart operations completed for health check cart: {CartId}", healthCartId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform cart operations for health check cart: {CartId}", healthCartId);
            
            // 尝试清理，即使操作失败
            try
            {
                await cartService.DeleteCartAsync(healthCartId);
            }
            catch
            {
                // 忽略清理错误
            }
        }
    }
}