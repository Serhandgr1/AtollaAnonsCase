using AnnounceMS.Entities.Entities;
using AnnounceMS.QueueProcessor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AnnounceMS.QueueProcessor.QueueService
{
    public class QueueManagerService : IQueueManagerService
    {
        private readonly Channel<(Announcement Announcement, TimeSpan? timeSpan)> setData;
        private readonly Channel<int> deleteData;
        private readonly Channel<(string cacheKey, TimeSpan? timeSpan, string serializedData)> updateData;
        public QueueManagerService()
        {
            int.TryParse("100", out int capacty);
            BoundedChannelOptions options = new(capacty)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            setData = Channel.CreateBounded<(Announcement Announcement, TimeSpan? timeSpan)>(options);
            deleteData = Channel.CreateBounded<int>(options);
            updateData = Channel.CreateBounded<(string cacheKey, TimeSpan? timeSpan, string serializedData)>(options);
        }
        public async Task SetCacheData(Announcement datas, TimeSpan? timeSpan = null)
        {
            ArgumentNullException.ThrowIfNull(datas);
            await setData.Writer.WriteAsync((datas, timeSpan));
        }
        public async Task DeleteCacheData(int id)
        {
            ArgumentNullException.ThrowIfNull(id);
            await deleteData.Writer.WriteAsync(id);
        }
        public async Task UpdateCacheData(string cacheKey, TimeSpan? timeSpan, string serializedData)
        {
            ArgumentNullException.ThrowIfNull(cacheKey);
            await updateData.Writer.WriteAsync((cacheKey, timeSpan, serializedData));
        }
        public async ValueTask<(Announcement Announcement, TimeSpan? timeSpan)> ReadSetCacheData(CancellationToken cancellationToken)
        {
            var setCacheData = await setData.Reader.ReadAsync(cancellationToken);
            return setCacheData;
        }
        public async ValueTask<int> ReadDeleteCacheData(CancellationToken cancellationToken)
        {
            var deleteCacheData = await deleteData.Reader.ReadAsync(cancellationToken);
            return deleteCacheData;
        }
        public async ValueTask<(string cacheKey, TimeSpan? timeSpan, string serializedData)> ReadUpdateCacheData(CancellationToken cancellationToken)
        {
            var updateCacheData = await updateData.Reader.ReadAsync(cancellationToken);
            return updateCacheData;
        }
    }
}
