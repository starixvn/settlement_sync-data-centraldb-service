using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutoSyncData.Services
{
    public interface AutoSyncDataController
    {
        Task ProcessAsync();
    }

    public class AutoSyncDataControllerImp: AutoSyncDataController
    {
        private readonly LogDetailService logDetailService;

        private readonly SyncDataController syncDataController;

        private readonly IConfiguration configuration;

        public AutoSyncDataControllerImp(LogDetailService logDetailService,
            SyncDataController syncDataController,
            IConfiguration configuration)
        {
            this.logDetailService = logDetailService;
            this.syncDataController = syncDataController;
            this.configuration = configuration;
        }

        public async Task ProcessAsync()
        {
            var servers = configuration.GetSection("SiteDBServer").Get<List<string>>();
            foreach(var server in servers)
            { 
                await syncDataController.Process(server);
            }
        }
    }
}
