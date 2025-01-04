using AnnounceMS.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnounceMS.Application.Interfaces
{
    public interface IAnnouncementService
    {
        Task<AnnouncementDTO> GetByIdAsync(int id);
        Task<IEnumerable<AnnouncementDTO>> GetAllUpcomingAsync();
        Task<IEnumerable<AnnouncementDTO>> GetExpiredAnnouncement();
        Task<AnnouncementDTO> CreateAsync(AnnouncementDTO createDto);
        Task<AnnouncementDTO> UpdateAsync(AnnouncementDTO updateDto);
        Task<bool> DeleteAsync(int id);
    }
}
