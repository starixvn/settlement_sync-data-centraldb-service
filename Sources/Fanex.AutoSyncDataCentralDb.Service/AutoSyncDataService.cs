using AutoSyncData.Services;
using Fanex.Data;
using Fanex.Data.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fanex.AutoSyncDataCentralDb
{
    public class AutoSyncDataService : IHostedService
    {
        private readonly AutoSyncDataController autoSyncDataController;

        private readonly IConfiguration configuration;

        private bool running;

        private int counter;

        public AutoSyncDataService(AutoSyncDataController autoSyncDataController,
            IConfiguration configuration)
        {
            this.autoSyncDataController = autoSyncDataController;
            this.configuration = configuration;
            InitializeSetting();
        }

        private void InitializeSetting()
        {
            StartDataBaseSetting();
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
            //while (running)
            //{
                //if (!maintenanceService.IsMaintenance)
                //{
                MainProcess();
                //Thread.Sleep(5000);
                //}
            //}
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
                Console.WriteLine(exception.Message);
                //Logging.LogService.Error("Exception in main thread - Process:", exception);
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

        private void StartDataBaseSetting()
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
        }
    }
}
