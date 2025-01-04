using AnnounceMS.QueueProcessor.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnounceMS.QueueProcessor.CacheQueueBackgroundServices
{
    public class DeleteCacheBackgroundService : BackgroundService
    {
        private readonly StackExchange.Redis.IDatabase _cache;
        private readonly IQueueManagerService _queueService;
        private readonly ILogger<DeleteCacheBackgroundService> _logger;

        public DeleteCacheBackgroundService(
            IQueueManagerService queueService,
            ILogger<DeleteCacheBackgroundService> logger,
             IConnectionMultiplexer redis)
        {
            _queueService = queueService;
            _logger = logger;
            _cache = redis.GetDatabase();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var announcement = await _queueService.ReadDeleteCacheData(stoppingToken);
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await RemoveAnnouncementFromCacheAsync(announcement);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Delete sirasinda hata olustu");
                        }
                    },stoppingToken);
                        
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing queue item");
                    await Task.Delay(1000, stoppingToken); // Hata durumunda bekle
                }
            }
        }
        public async Task RemoveAnnouncementFromCacheAsync(int id)
        {
            string cacheKey = $"announcement:{id}";
            await _cache.KeyDeleteAsync(cacheKey);
        }
    }
}
