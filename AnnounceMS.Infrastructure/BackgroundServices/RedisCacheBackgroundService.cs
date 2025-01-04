using AnnounceMS.Entities.Entities;
using AnnounceMS.Infrastructure.Interfaces;
using AnnounceMS.Infrastructure.RepositoriesManager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace AnnounceMS.Infrastructure.BackgroundServices
{
    public class RedisCacheBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<RedisCacheBackgroundService> _logger;
        private readonly IDatabase _cache;

        public RedisCacheBackgroundService(ILogger<RedisCacheBackgroundService> logger, IConnectionMultiplexer redis, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _cache = redis.GetDatabase();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // İlk çalıştırmayı hemen yap
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await UpdateCacheAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ilk Cache sirasinda hata olustu");
                    }
                }, stoppingToken);


                while (!stoppingToken.IsCancellationRequested)
                {
                    // Bir sonraki gün 00:00'ı hesapla
                    var now = DateTime.Now;
                    var nextRun = now.Date.AddDays(1);
                    var delay = nextRun - now;

                    _logger.LogInformation("Next cache update scheduled for: {time}", nextRun);

                    // Bir sonraki çalışma zamanını bekle
                    await Task.Delay(delay, stoppingToken);

                    // Saat 00:00 olduğunda çalıştır
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await UpdateCacheAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "00 Cache guncelleme istemi basarisiz oldu");
                        }
                    }, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Message RedisCacheBackgoundService {ex.Message}");
            }

        }
        //performans kontrol
        private async Task UpdateCacheAsync()
        {
            try
            {
                await ClearAllCache();
                var data = await GetCacheData();
                foreach (var announcement in data)
                {
                    string cacheKey = $"announcement:{announcement.Id}";
                    string serializedData = JsonSerializer.Serialize(announcement);
                  
                    await _cache.StringSetAsync(cacheKey, serializedData);
                }
                _logger.LogInformation("Caching Completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during cache update");
            }
        }
        private async Task ClearAllCache()
        {
            try
            {
                // Redis sunucusuna bağlan
                var server = _cache.Multiplexer.GetServer(
                    _cache.Multiplexer.GetEndPoints().First()
                );

                // "announcement:*" pattern'ine uyan tüm keyleri al
                var keys = server.Keys(pattern: "announcement:*").ToArray();

                // Tüm keyleri sil
                if (keys.Any())
                {
                    await _cache.KeyDeleteAsync(keys);
                    _logger.LogInformation(
                        "Cleared {count} announcements from cache",
                        keys.Length
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
                throw;
            }
        }
        private async Task<List<Announcement>> GetCacheData() 
        {
            using var scope = _serviceProvider.CreateScope();

            // Scope içinden servisleri al
            var repositoryManager = scope.ServiceProvider
                .GetRequiredService<IRepositoryManager>();
            var today = DateTime.Now.Date;
            var startDateThreshold = today.AddDays(-10);
            var announcements = await repositoryManager.AnnouncementRepository.GenericReadExpression(x => x.StartDate >= startDateThreshold && x.StartDate <= today && x.EndDate > today,false).ToListAsync(); 
            return announcements;
        }
    }
}
