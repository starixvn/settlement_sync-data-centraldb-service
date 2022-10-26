using AutoSyncData.Services;
using Fanex.Caching;
using Fanex.Data;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.AspNetCore;
using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.MailDeliver.MailClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Transport;
using SharpRaven;

namespace Fanex.AutoSyncDataCentralDb;

public class AutoSyncDataService : IHostedService
{
    private readonly AutoSyncDataController autoSyncDataController;

    private readonly IConfiguration configuration;

    public AutoSyncDataService(
        AutoSyncDataController autoSyncDataController,
        IConfiguration configuration)
    {
        this.autoSyncDataController = autoSyncDataController;
        this.configuration = configuration;
        InitializeExtraComponent();
        InitializeSetting();
    }

    private void InitializeSetting()
    {
        StartUmSetting();
        ConfigureFanex();
        ConfigureMailSetting();
    }

    private static void InitializeExtraComponent()
    {
        TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;
    }

    private static void TaskSchedulerUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved();
        e.Exception.Handle(x =>
        {
            LogService.Error(x.Message, x);
            return true;
        });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        MainProcess();
        Console.WriteLine($"Auto Sync Data Starting at {DateTime.Now}");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"Auto Sync Data Stopping at {DateTime.Now}");
        return Task.CompletedTask;
    }

    private void MainProcess()
    {
        try
        {
            autoSyncDataController.ProcessAsync();
        }
        catch (Exception exception)
        {
            LogService.Error("AutoSyncDataCentralDb - Exception in main thread - Process:", exception);
        }
    }

    private void StartUmSetting()
    {
        var umConfig = new UMClientConfiguration().Initialize(configuration);

        UMClientManager
            .UseConfig(umConfig)
            .UseCacheService(new UMCacheService(new CacheService()))
            .UseRestClient(new UMRestClient(umConfig));
    }

    private void ConfigureFanex()
    {
        var connections = configuration.GetSection("ConnectionStrings")
            .GetChildren()
            .AsEnumerable()
            .ToDictionary(
                x => x.GetValue<string>("Name"),
                y => new ConnectionConfiguration(y.GetValue<string>("Name"), y.GetValue<string>("ConnectionString"), y.GetValue<string>("ProviderName"))
            );

        DbSettingProviderManager
            .StartNewSession()
            .Use(connections)
            .WithSqlServer()
            .UseDefaultDbSettingProvider(resourcePath: configuration.GetSection("StoredProcedurePath").Value)
            .Run();

        LogManager
            .SetDefaultLogCategory(configuration.GetSection("SiteCategory").Value)
            .Use(new List<ILogEngine>
            {
                    new SentryLogging(new SentryEngineOptions
                    {
                        Dsn = new Dsn(configuration.GetSection("SentryToken").Value)
                    })
            });

        ExceptionHandler
            .UseConfig(new LoggingConfiguration())
            .UseGetDbExceptionInfo(DatabaseInfoGetters.GetDbExceptionInfo);
    }

    private void ConfigureMailSetting()
    {
        AppSettings.AppMailId = configuration.GetSection("MailServer:Id").Value;
        AppSettings.AppMailAddress = configuration.GetSection("MailServer:Address").Value;
    }
}