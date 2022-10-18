using Fanex.Caching;

namespace AutoSyncData.Services
{
    public interface ControlSyncService
    {
        Task<bool> IsStartService();
    }

    public class ControlSyncServiceImp : ControlSyncService
    {
        private const string AutoSyncKey = "Settlement-Auto-Sync-Sportsbook";
        private readonly ICache cache;

        public ControlSyncServiceImp(ICache cache)
        {
            this.cache = cache;
        }

        public async Task<bool> IsStartService()
        {
            try
            {
                var isExistedKey = await cache.ContainAsync(AutoSyncKey);
                if (isExistedKey)
                {
                    var (_, value) = await cache.TryGetAsync<bool>(AutoSyncKey);
                    return value;
                }

                return true;
            }
            catch
            {
                return true;
            }
        }
    }
}
