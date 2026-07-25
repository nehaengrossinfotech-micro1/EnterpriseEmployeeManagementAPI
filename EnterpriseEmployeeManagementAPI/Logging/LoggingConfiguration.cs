using Serilog;
using Serilog.Formatting.Compact;

namespace EnterpriseEmployeeManagementAPI.Logging;

public static class LoggingConfiguration
{
    public static LoggerConfiguration Configure(
        LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        return loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "EnterpriseEmployeeManagementAPI")
            .Enrich.WithProperty("Environment", environment.EnvironmentName)
            .WriteTo.Console(new RenderedCompactJsonFormatter());
    }
}
