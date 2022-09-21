using AutoSyncData.Services;
using Fanex.Data;
using Fanex.Data.SqlServer;
using Fanex.MailDeliver.MailClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ONEbook.UM.Configurations;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Transport;
using Fanex.Caching;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.AspNetCore;
using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Sentry;
using Sentry;

namespace Fanex.AutoSyncDataCentralDb
{
    public class AutoSyncDataService : IHostedService
    {
        private readonly AutoSyncDataController autoSyncDataController;

        private readonly IConfiguration configuration;

        private readonly IMaintenanceService maintenanceService;

        private bool running;

        private int counter;

        public AutoSyncDataService(AutoSyncDataController autoSyncDataController,
            IConfiguration configuration,
            IMaintenanceService maintenanceService)
        {
            this.autoSyncDataController = autoSyncDataController;
            this.configuration = configuration;
            this.maintenanceService = maintenanceService;
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
            running = true;
            var autoPayoutThread = new Thread(ThreadProcess);
            Console.WriteLine($"Auto Sync Data Starting at { DateTime.Now }");
            autoPayoutThread.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            running = false;
            Console.WriteLine($"Auto Sync Data Stopping at { DateTime.Now }");
            return Task.CompletedTask;
        }

        private void ThreadProcess()
        {
            while (running)
            {
                if (!maintenanceService.IsMaintenance)
                {
                    MainProcess();
                    Thread.Sleep(int.Parse(configuration.GetSection("ServiceInterval").Value));
                }
            }
        }

        private void MainProcess()
        {
            try
            {
                autoSyncDataController.ProcessAsync();
                HandleGarbageCollector();
            }
            catch (Exception exception)
            {
                LogService.Error("Exception in main thread - Process:", exception);
            }
        }

        private void HandleGarbageCollector()
        {
            if (counter >= int.Parse(configuration.GetSection("GarbageCollectorInterval").Value))
            {
                counter = 0;
                GC.WaitForPendingFinalizers();
            }

            counter += int.Parse(configuration.GetSection("ServiceInterval").Value);
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
                .SetDefaultLogCategory(configuration.GetSection("LogCategory").Value)
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
}
