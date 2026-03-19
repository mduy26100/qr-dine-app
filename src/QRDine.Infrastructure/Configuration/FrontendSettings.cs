using QRDine.Application.Common.Abstractions.Configurations;

namespace QRDine.Infrastructure.Configuration
{
    public class FrontendSettings
    {
        public string BaseUrl { get; set; } = default!;
    }

    public class FrontendConfigProvider : IFrontendConfig
    {
        private readonly FrontendSettings _settings;

        public FrontendConfigProvider(IOptions<FrontendSettings> options)
        {
            _settings = options.Value;
        }

        public string BaseUrl => _settings.BaseUrl;
    }
}
