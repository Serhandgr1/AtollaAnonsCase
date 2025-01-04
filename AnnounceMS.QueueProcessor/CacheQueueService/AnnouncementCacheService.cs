using AnnounceMS.Entities.Entities;
using AnnounceMS.Infrastructure.RepositoriesManager;
using AnnounceMS.QueueProcessor.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AnnounceMS.QueueProcessor.CacheQueueService
{

    public class AnnouncementCacheService : IAnnouncementCacheService
    {
        private readonly IQueueManagerService _queueService;
        private readonly StackExchange.Redis.IDatabase _cache;
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILogger<AnnouncementCacheService> _logger;

        public AnnouncementCacheService(IConnectionMultiplexer redis, IRepositoryManager repositoryManager, IQueueManagerService queueService, ILogger<AnnouncementCacheService> logger)
        {
            _cache = redis.GetDatabase();
            _repositoryManager = repositoryManager;
            _queueService = queueService;
            _logger = logger;
        }

        // Veriyi id ile ve aktif yada pasif duyuru olarak istenilen secenege gore getir 
        public async Task<Announcement> GetByIdAnnouncementAsync(int id)
        {
            try {
                string cacheKey = $"announcement:{id}";
                // Redis'ten oku
                var cachedData = await _cache.StringGetAsync(cacheKey);
                if (!cachedData.IsNullOrEmpty)
                {
                    // Redis'te varsa deserialize edip döndür
                    return JsonSerializer.Deserialize<Announcement>(cachedData!);
                }

                return null;
            } catch (Exception ex) {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public async Task<List<Announcement>> GetAllUpcomingAnnouncements()
        {
            try {
                var multiplexer = _cache.Multiplexer;
                var server = multiplexer.GetServer(multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: "announcement:*").ToList();
                var announcements = new List<Announcement>();
                foreach (var key in keys)
                {
                    try
                    {
                        var value = await _cache.StringGetAsync(key);
                        if (!value.IsNullOrEmpty)
                        {
                            var announcement = JsonSerializer.Deserialize<Announcement>(value!);
                            if (announcement is not null)
                            {
                                announcements.Add(announcement);
                            }
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError($"Deserialize isleminde hata olustu {key}: {jsonEx.Message}");
                    }
                }
                if (announcements.Count == 0) {
                    return default(List<Announcement>);
                }
                return announcements;
            } catch (Exception ex) {
                _logger.LogError(ex.Message);
                return default(List<Announcement>);
            }
            
        }

        public async Task<bool> UpdateAnnouncementInCacheAsync(Announcement announcement)
        {
            try
            {
                if (announcement is not null)
                {
                    var currentDateTime = DateTime.Now.AddTicks(-(DateTime.Now.Ticks % TimeSpan.TicksPerSecond));
                    var endDate = announcement.EndDate.AddTicks(-(announcement.EndDate.Ticks % TimeSpan.TicksPerSecond));
                    var startDate = announcement.StartDate.AddTicks(-(announcement.StartDate.Ticks % TimeSpan.TicksPerSecond));
                    var startDateThreshold = currentDateTime.AddDays(-10);
                    // Cache key oluştur
                    string cacheKey = $"announcement:{announcement.Id}";

                    // Önce cache'te var mı kontrol et
                    var existingData = await _cache.StringGetAsync(cacheKey);
                    if (!existingData.IsNullOrEmpty)
                    {
                        //Eger guncellenmek istenen veri cachete varsa ama guncellendiktan sonra cachte olmamasi gereken kosullara sahip olmussa cacheten siliyorum
                        if (endDate <= currentDateTime || startDate > currentDateTime || startDate < startDateThreshold)
                        {
                            await _queueService.DeleteCacheData(announcement.Id);
                        }
                        else
                        {
                            var remainingTtl = await _cache.KeyTimeToLiveAsync(cacheKey);
                            // Yeni veriyi serialize et ve güncelle
                            string serializedData = JsonSerializer.Serialize(announcement);
                            await _queueService.UpdateCacheData(cacheKey, remainingTtl, serializedData);
                        }

                    }
                    else if (existingData.IsNullOrEmpty && startDate >= startDateThreshold && startDate <= currentDateTime && endDate >= currentDateTime)
                    {
                        await _queueService.SetCacheData(announcement);
                    }
                    return true;
                }
                else return false;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        //donus yapmasina gerek yok
        public async Task<Announcement> CreateAnnouncement(Announcement announcement)
        {
            var currentDateTime = DateTime.Now.AddTicks(-(DateTime.Now.Ticks % TimeSpan.TicksPerSecond));
            var endDate = announcement.EndDate.AddTicks(-(announcement.EndDate.Ticks % TimeSpan.TicksPerSecond));
            var startDate = announcement.StartDate.AddTicks(-(announcement.StartDate.Ticks % TimeSpan.TicksPerSecond));
            var startDateThreshold = currentDateTime.AddDays(-10);
                //Kaydedilmek istenen verinin baslangic tarihi bugun ile 10 gun sonrasi arasinda ise veriyi cache da tutmak icin queen (kuyruga) gonderir
                if (startDate >= startDateThreshold && startDate <= currentDateTime && endDate >= currentDateTime)
                {
                    await _queueService.SetCacheData(announcement);
                }
                return announcement;
        }
        public async Task<bool> DeleteAnnouncement(int id)
        {
            string cacheKey = $"announcement:{id}";
            // Veri Cachete var mi 
            var cachedData = await _cache.StringGetAsync(cacheKey);
            // Var ise cache ten veri siliniyor 
            if (!cachedData.IsNullOrEmpty)
            {
                await _queueService.DeleteCacheData(id);
                return true;    
            }
            else return false;
        }
    }
}
