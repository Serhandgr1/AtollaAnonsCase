using AnnounceMS.Entities.Entities;
using AnnounceMS.Infrastructure.Context;
using AnnounceMS.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnounceMS.Infrastructure.Repositories
{
    public class AnnouncementRepository : GenericRepository<Announcement> , IAnnouncementRepository
    {
        private readonly DataContext _context;
        public AnnouncementRepository(DataContext context) : base(context) => _context = context;
    }
}
