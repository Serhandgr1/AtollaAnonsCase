using AnnounceMS.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnounceMS.Infrastructure.RepositoriesManager
{
    public interface IRepositoryManager
    {
        IAnnouncementRepository AnnouncementRepository { get; }
        void Save();
    }
}
