namespace SharedKernel.Caching.Settings
{
    public class RedisSettings
    {
        public const string SectionName = "RedisSettings";

        public string ConnectionString { get; set; } = string.Empty;
        public int ConnectTimeoutMs { get; set; } = 150;
        public int SyncTimeoutMs { get; set; } = 150;
        public int AsyncTimeoutMs { get; set; } = 150;
        public bool AbortOnConnectFail { get; set; } = false;

        public int CircuitBreakerCooldownSeconds { get; set; } = 30;
        public int L1SurvivalDurationSeconds { get; set; } = 60;
    }
}
