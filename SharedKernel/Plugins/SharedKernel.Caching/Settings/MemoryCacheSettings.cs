namespace SharedKernel.Caching.Settings
{
    public class MemoryCacheSettings
    {
        public const string SectionName = "MemoryCacheSettings";

        public long? SizeLimit { get; set; }

        public int ExpirationScanFrequencySeconds { get; set; } = 60;
    }
}
