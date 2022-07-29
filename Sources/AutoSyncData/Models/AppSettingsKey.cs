namespace AutoSyncData.Models
{
    public class AppSettingsKey
    {
        public int RetryCount { get; set; }

        public int RetryInterval { get; set; }

        public string EnvironmentName { get; set; }

        public string DeploymentDelay { get; set; }
    }
}
