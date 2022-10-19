using AutoSyncData.Services;
using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Transport;

namespace Fanex.AutoSyncDataCentralDb.Service;

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
        service.AddTransient<IMaintenanceService, MaintenanceServiceImp>();
        service.AddSingleton<SyncDataHandler, SyncDataHandlerImp>();
        service.AddSingleton(new UMClientConfiguration().Initialize(configuration));
        service.AddSingleton<IUMCacheService, UMCacheService>();
        service.AddSingleton<IUMRestClient, UMRestClient>();
        service.AddSingleton<ONEbook.UM.Services.IMaintenanceService, ONEbook.UM.Services.MaintenanceService>();
        service.AddConfigRedisCache(configuration);
    }

    private static IServiceCollection AddConfigRedisCache(this IServiceCollection service, IConfiguration configuration)
    {
        var redisConfig = configuration.GetSection("AuthSettings:RedisConfig").Value;
        var applicationName = configuration.GetSection("AuthSettings:ChannelPrefix").Value;
        service.AddSingleton<ICache>(new RedisCache(new RedisCacheOptions
        {
            ConnectionString = redisConfig,
            ApplicationName = applicationName,
        }));
        service.AddSingleton<ICacheService>(new CacheService());

        return service;
    }
}