using AutoSyncData.Services;
using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fanex.AutoSyncDataCentralDb.Service
{
    public static class AutoSyncDataInjector
    {
        public static void InitializeServices(this IServiceCollection service, IConfiguration configuration)
        {
            service.AddSingleton<IDynamicRepository, DynamicRepository>();
            service.AddSingleton<AutoSyncDataController, AutoSyncDataControllerImp>();
            service.AddSingleton<LogDetailService, LogDetailServiceImp>();
            service.AddSingleton<SendMailService, SendMailServiceImp>();
            service.AddSingleton<SyncDataController, SyncDataControllerImp>();
            service.AddSingleton<SyncDataService, SyncDataServiceImp>();
            service.AddSingleton<ValidationService, ValidationServiceImp>();
            ConfigurRedis(service, configuration);
        }

        private static void ConfigurRedis(IServiceCollection service, IConfiguration configuration)
        {
            var redisHost = configuration.GetSection("Redis.Server").Value;
            var redisPort = configuration.GetSection("Redis.Port").Value;
            var redisPassword = configuration.GetSection("Redis.Password").Value;
            var redisDatabase = configuration.GetSection("Redis.DefaultDatabase").Value;
            service.AddSingleton<ICache>(new RedisCache(new RedisCacheOptions
            {
                ConnectionString = $"{redisHost}:{redisPort},password={redisPassword},defaultDatabase={redisDatabase}",
                ApplicationName = configuration.GetSection("Redis.ApplicationName").Value,
            }));
            service.AddSingleton<ICacheService>(new CacheService());
        }
    }
}
