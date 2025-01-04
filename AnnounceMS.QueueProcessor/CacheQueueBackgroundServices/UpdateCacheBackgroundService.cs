using AnnounceMS.Entities.Entities;
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
    public class UpdateCacheBackgroundService : BackgroundService
    {
        private readonly StackExchange.Redis.IDatabase _cache;
        private readonly IQueueManagerService _queueService;
        private readonly ILogger<UpdateCacheBackgroundService> _logger;

        public UpdateCacheBackgroundService(
            IQueueManagerService queueService,
            ILogger<UpdateCacheBackgroundService> logger,
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
                    var datas = await _queueService.ReadUpdateCacheData(stoppingToken);
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await SetAnnouncementToCacheAsync(datas.cacheKey, datas.timeSpan, datas.serializedData);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Update sirasinda hata olustu");
                        }
                    }, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
        }
        public async Task SetAnnouncementToCacheAsync(string cacheKey, TimeSpan? timeSpan, string serializedData)
        {
            try
            {
                await _cache.StringSetAsync(cacheKey, serializedData, timeSpan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
