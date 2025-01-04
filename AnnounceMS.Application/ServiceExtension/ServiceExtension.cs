using AnnounceMS.Application.Interfaces;
using AnnounceMS.Application.Services;
using AnnounceMS.Application.ServicesManager;
using AnnounceMS.Infrastructure.Context;
using AnnounceMS.Infrastructure.Interfaces;
using AnnounceMS.Infrastructure.Repositories;
using AnnounceMS.Infrastructure.RepositoriesManager;
using AnnounceMS.QueueProcessor.CacheQueueService;
using AnnounceMS.QueueProcessor.Interfaces;
using AnnounceMS.QueueProcessor.QueueService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;




namespace AnnounceMS.Application.ServiceExtension
{
    public static class ServiceExtension
    {
        public static void ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
        {
              services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),b => b.MigrationsAssembly("AnnounceMS.Infrastructure")));
        }
        public static void ConfiguerRepostoryManager(this IServiceCollection services)
        {
            //BaseGenericRepositoryReferance
            services.AddScoped<IRepositoryManager, RepositoryManager>();
            services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        }
        public static void ConfigureRedisServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Redis connection
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisConnection = configuration["RedisSettings:RedisConnectionStrings"];
                return ConnectionMultiplexer.Connect(redisConnection);
            });
            services.AddStackExchangeRedisCache
                (opt =>
                {
                    opt.Configuration = configuration["RedisSettings:RedisConnectionStrings"];
                    opt.InstanceName = configuration["RedisSettings:RedisInstanceName"];
                });

        }
        public static void ConfiguerServiceManager(this IServiceCollection services)
        {
            services.AddSingleton<IQueueManagerService, QueueManagerService>();
            services.AddScoped<IServiceManager, ServiceManager>();
            services.AddScoped<QueueProcessor.Interfaces.IAnnouncementCacheService, AnnouncementCacheService>();
            services.AddScoped<IAnnouncementService, AnnouncementService>();
        }
    }
}
