using AnnounceMS.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnounceMS.QueueProcessor.Interfaces
{
    public interface IAnnouncementCacheService
    {
        Task<Announcement> GetByIdAnnouncementAsync(int id);
        Task<List<Announcement>> GetAllUpcomingAnnouncements();
        Task<bool> UpdateAnnouncementInCacheAsync(Announcement announcement);
        Task<Announcement> CreateAnnouncement(Announcement announcement);

        Task<bool> DeleteAnnouncement(int id);
    }
}
