using AnnounceMS.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnounceMS.QueueProcessor.Interfaces
{
    public interface IQueueManagerService
    {
        Task UpdateCacheData(string cacheKey, TimeSpan? timeSpan, string serializedData);
        Task SetCacheData(Announcement Announcement, TimeSpan? timeSpan = null);
        Task DeleteCacheData(int id);
        ValueTask<(Announcement Announcement, TimeSpan? timeSpan)> ReadSetCacheData(CancellationToken cancellationToken);
        ValueTask<int> ReadDeleteCacheData(CancellationToken cancellationToken);
        ValueTask<(string cacheKey, TimeSpan? timeSpan, string serializedData)> ReadUpdateCacheData(CancellationToken cancellationToken);
    }
}
