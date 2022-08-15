using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SensorBatteryChecker;
using SensorBatteryChecker.Configurations;
using SensorBatteryChecker.Services.Battery;
using SensorBatteryChecker.Services.Mail;

[assembly: FunctionsStartup(typeof(Startup))]

namespace SensorBatteryChecker;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddLogging();

        // Configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        builder.Services.AddSingleton<IConfiguration>(config);

        builder.Services.AddOptions<SendGridConfiguration>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("SendGrid").Bind(settings);
            });
        builder.Services.AddOptions<TelldusConfiguration>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("Telldus").Bind(settings);
            });

        // Services
        builder.Services.AddTransient<IMailService, SendGridService>();
        builder.Services.AddTransient<IBatteryService, BatteryService>();
    }
}
