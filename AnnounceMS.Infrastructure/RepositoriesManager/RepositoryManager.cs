using AnnounceMS.Infrastructure.Context;
using AnnounceMS.Infrastructure.Interfaces;
using AnnounceMS.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnounceMS.Infrastructure.RepositoriesManager
{
    public class RepositoryManager : IRepositoryManager
    {
        private readonly DataContext _context;
        private readonly Lazy<IAnnouncementRepository> _announcementRepository;

        public RepositoryManager(DataContext context)
        {
            _context = context;
            _announcementRepository = new Lazy<IAnnouncementRepository>(new AnnouncementRepository(_context));
  
        }
      public IAnnouncementRepository AnnouncementRepository => _announcementRepository.Value;
     
        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
