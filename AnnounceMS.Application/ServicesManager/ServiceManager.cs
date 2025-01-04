using AnnounceMS.Infrastructure.RepositoriesManager;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnounceMS.Application.ServicesManager
{
    public class ServiceManager : IServiceManager
    {
        public ServiceManager(IRepositoryManager repository, IMapper mapper) //ILogger<T> loggerDevices
        {

        }
    }
}
