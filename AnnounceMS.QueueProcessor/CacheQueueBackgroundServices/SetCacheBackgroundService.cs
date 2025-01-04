using AnnounceMS.Entities.Entities;
using AnnounceMS.QueueProcessor.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AnnounceMS.QueueProcessor.CacheQueueBackgroundServices
{
    public class SetCacheBackgroundService : BackgroundService
    {
        private readonly StackExchange.Redis.IDatabase _cache;
        private readonly IQueueManagerService _queueService;
        private readonly ILogger<SetCacheBackgroundService> _logger;

        public SetCacheBackgroundService(
            IQueueManagerService queueService,
            ILogger<SetCacheBackgroundService> logger,
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
                    var announcement = await _queueService.ReadSetCacheData(stoppingToken);
                    // Cache'e ekle
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await SetAnnouncementToCacheAsync(announcement.Announcement, announcement.timeSpan);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Delete sirasinda hata olustu");
                        }
                    }, stoppingToken);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogError(ex.Message, "OperationCanceledException");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kuyruk öğesi işlenirken hata oluştu");
                    await Task.Delay(1000, stoppingToken); // Hata durumunda bekle
                }
            }
        }
        public async Task SetAnnouncementToCacheAsync(Announcement announcement, TimeSpan? timeSpan = null)
        {
            try
            {
                string cacheKey = $"announcement:{announcement.Id}";
                string serializedData = JsonSerializer.Serialize(announcement);
                await _cache.StringSetAsync(cacheKey, serializedData, timeSpan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

        }
    }
}
