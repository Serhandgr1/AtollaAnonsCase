using AnnounceMS.Application.DTOs;
using AnnounceMS.Application.Interfaces;
using AnnounceMS.Entities.Entities;
using AnnounceMS.Infrastructure.RepositoriesManager;
using AnnounceMS.QueueProcessor.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace AnnounceMS.Application.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementCacheService _cacheService;
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;
        private readonly ILogger<AnnouncementService> _logger;
        public AnnouncementService(
            IAnnouncementCacheService cacheService,
        IMapper mapper,
            ILogger<AnnouncementService> logger,
            IRepositoryManager repositoryManager
          )
        {
            _cacheService = cacheService;
            _mapper = mapper;
            _logger = logger;
            _repositoryManager = repositoryManager;
        }

        public async Task<AnnouncementDTO> GetByIdAsync(int id)
        {
            try
            {
                var announcement = await _cacheService.GetByIdAnnouncementAsync(id);
                if (announcement == null)
                {
                     var data= await _repositoryManager.AnnouncementRepository.GenericReadExpression(x=>x.Id==id,false).FirstOrDefaultAsync();
                    if (data == null) {
                        throw new Exception($"ID {id} veri bulunamadi");
                    }
                    else return _mapper.Map<AnnouncementDTO>(data);

                }

                return _mapper.Map<AnnouncementDTO>(announcement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting announcement {Id}", id);
                throw;
            }
        }
        public async Task<IEnumerable<AnnouncementDTO>> GetExpiredAnnouncement()
        {
            try
            {
                var currentDateTime = DateTime.Now.AddTicks(-(DateTime.Now.Ticks % TimeSpan.TicksPerSecond));
                var announcements =  await _repositoryManager.AnnouncementRepository.GenericReadExpression(x=>x.EndDate< currentDateTime, false).ToListAsync();
                if (announcements == null) {
                    throw new Exception($"Veri bulunamadi");
                }
                return _mapper.Map<IEnumerable<AnnouncementDTO>>(announcements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming announcements");
                throw;
            }
        }
        public async Task<IEnumerable<AnnouncementDTO>> GetAllUpcomingAsync()
        {
            try
            {
                var announcements = await _cacheService.GetAllUpcomingAnnouncements();
                if (announcements == null) {
                    throw new Exception($"Veri bulunamadi");
                }
                return _mapper.Map<IEnumerable<AnnouncementDTO>>(announcements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yaklasan duyurular alinamadi");
                throw;
            }
        }

        public async Task<AnnouncementDTO> CreateAsync(AnnouncementDTO createDto)
        {
            try
            {
                var currentDateTime = DateTime.Now.AddTicks(-(DateTime.Now.Ticks % TimeSpan.TicksPerSecond));
                var endDate = createDto.EndDate.AddTicks(-(createDto.EndDate.Ticks % TimeSpan.TicksPerSecond));
                var startDate = createDto.StartDate.AddTicks(-(createDto.StartDate.Ticks % TimeSpan.TicksPerSecond));
                if (endDate <= currentDateTime)
                {
                    throw new ValidationException("Duyurunun bitis tarihi su an`dan eski olamaz bu durum duyuru eklenmeden suresinin doldugu anlamina gelir");
                }
                if (endDate <= startDate) {
                    throw new ValidationException("Duyurunun bitis tarihi duyurunun baslama tarihinden once olamaz bu durum duyuru baslamadan once bitecek anlamina gelir");
                }
                var announcement = _mapper.Map<Announcement>(createDto);
                var created = await _repositoryManager.AnnouncementRepository.GenericCreate(announcement); 
                if (created is not null) {
                    _repositoryManager.Save();
                    await _cacheService.CreateAnnouncement(created);
                }
                
                return _mapper.Map<AnnouncementDTO>(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Duyuru oluşturulurken hata oluştu");
                throw;
            }
        }

        public async Task<AnnouncementDTO> UpdateAsync(AnnouncementDTO updateDto)
        {
            try
            {
                var endDate = updateDto.EndDate.AddTicks(-(updateDto.EndDate.Ticks % TimeSpan.TicksPerSecond));
                var startDate = updateDto.StartDate.AddTicks(-(updateDto.StartDate.Ticks % TimeSpan.TicksPerSecond));

                if (endDate <= startDate)
                {
                    throw new ValidationException("Duyurunun bitis tarihi duyurunun baslama tarihinden once olamaz bu durum duyuru baslamadan once bitecek anlamina gelir");
                }
                var announcement = _mapper.Map<Announcement>(updateDto);
                _repositoryManager.AnnouncementRepository.GenericUpdate(announcement);
                _repositoryManager.Save();
               
                var success = await _cacheService.UpdateAnnouncementInCacheAsync(announcement);
                if (!success)
                {
                    throw new Exception($"Guncellenirken bir hata olustu {updateDto.Id}");
                }

                return _mapper.Map<AnnouncementDTO>(announcement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Duyuru güncellenirken hata oluştu {Id}", updateDto.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var announcement = await _repositoryManager.AnnouncementRepository.GenericReadExpression(x => x.Id == id, false).FirstOrDefaultAsync();
                // Varsa Sql serverdan veri siliniyor
                if (announcement is not null)
                {
                    _repositoryManager.AnnouncementRepository.GenericDelete(announcement);
                    _repositoryManager.Save();
                }
                bool isDeleted=  await _cacheService.DeleteAnnouncement(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Duyuru silinirken hata olustu {Id}", id);
                return false;
            }
        }
    }
}
