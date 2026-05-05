namespace QRDine.API.DependencyInjection.Presentation
{
    public static class LoggingServiceCollectionExtensions
    {
        public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();

            var seqServerUrl = builder.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341";
            var seqApiKey = builder.Configuration["Seq:ApiKey"];

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .MinimumLevel.Information()

                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)

                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)

                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Information)

                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("ApplicationName", "QRDine.API")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}")
                .WriteTo.Seq(seqServerUrl, apiKey: seqApiKey)
                .CreateLogger();

            builder.Host.UseSerilog();

            return builder;
        }
    }
}
