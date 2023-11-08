using AutoMapper;
using AutoSyncData.Services;
using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.AspNetCore.Http;
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
        service.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        service.AddSingleton<GeneralService, GeneralServiceImp>();
        service.AddSingleton<ScaleOutSettleService, ScaleOutSettleServiceImp>();

        service.AddSingleton<IDynamicRepository, DynamicRepository>();
        service.AddSingleton<AutoSyncDataController, AutoSyncDataControllerImp>();
        service.AddSingleton<QueryLogService, QueryLogServiceImp>();
        service.AddSingleton<SyncDataController, SyncDataControllerImp>();
        service.AddSingleton<SyncDataService, SyncDataServiceImp>();
        service.AddSingleton<SyncDataHandler, SyncDataHandlerImp>();
        service.AddSingleton<SendMailService, SendMailServiceImp>();

        service.AddSingleton(new UMClientConfiguration().Initialize(configuration));
        service.AddSingleton<IUMCacheService, UMCacheService>();
        service.AddSingleton<IUMRestClient, UMRestClient>();
        service.AddSingleton<ONEbook.UM.Services.IMaintenanceService, ONEbook.UM.Services.MaintenanceService>();
        service.AddConfigRedisCache(configuration);
        service.AddAutoMapper();
    }

    private static void AddAutoMapper(this IServiceCollection services)
    {
        var mapperConfig = new MapperConfiguration(_ => { });

        var mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);
    }

    private static void AddConfigRedisCache(this IServiceCollection service, IConfiguration configuration)
    {
        var redisConfig = configuration.GetSection("AuthSettings:RedisConfig").Value;
        var applicationName = configuration.GetSection("AuthSettings:ChannelPrefix").Value;
        service.AddSingleton<ICache>(new RedisCache(new RedisCacheOptions
        {
            ConnectionString = redisConfig,
            ApplicationName = applicationName,
        }));
        service.AddSingleton<ICacheService>(new CacheService());
    }
}