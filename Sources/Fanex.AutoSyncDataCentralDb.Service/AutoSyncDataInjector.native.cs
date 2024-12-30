using AutoSyncData.Services;
using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fanex.AutoSyncDataCentralDb.Service;

public static class AutoSyncDataInjector
{
    public static void InitializeServices(this IServiceCollection service, IConfiguration configuration)
    {
        service.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        service.AddSingleton<IDynamicRepository, DynamicRepository>();
        service.AddSingleton<AutoSyncDataController, AutoSyncDataControllerImp>();
        service.AddSingleton<QueryLogService, QueryLogServiceImp>();
        service.AddSingleton<SyncDataController, SyncDataControllerImp>();
        service.AddSingleton<SyncDataService, SyncDataServiceImp>();
        service.AddSingleton<SyncDataHandler, SyncDataHandlerImp>();
    }

    public static void AddConfigRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("Redis:ConnectionString");
        var applicationName = configuration.GetValue<string>("Redis:ApplicationName");

        services.AddSingleton<ICache>(new RedisCache(new RedisCacheOptions
        {
            ConnectionString = connectionString,
            ApplicationName = applicationName,
        }));
        services.AddSingleton<ICacheService>(new CacheService());
    }
}