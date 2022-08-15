using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SensorBatteryChecker.Configurations;
using SensorBatteryChecker.Factories;
using SensorBatteryChecker.Services.Mail;
using Wolfberry.TelldusLive;
using Wolfberry.TelldusLive.Models.Sensor;

namespace SensorBatteryChecker.Services.Battery;

public interface IBatteryService
{
    Task CheckBatteryAsync();
}

public class BatteryService : IBatteryService
{
    private readonly ILogger<BatteryService> _logger;
    private readonly IMailService _mailService;
    private readonly ITelldusLiveClient _telldusClient;

    public BatteryService(
        ILogger<BatteryService> logger, 
        IMailService mailService,
        IOptions<TelldusConfiguration> telldusOptions)
    {
        _logger = logger;
        _mailService = mailService;

        // TODO: Add dependency injection
        try
        {
            _telldusClient = TelldusClientFactory.CreateTelldusLiveClient(telldusOptions.Value);
        }
        catch (Exception e)
        {
            // TODO: Add notification through Azure Monitor instead of sending mail here
            const string Msg = "Failed to initialize Telldus client. Check configuration";
            logger.LogError(e, Msg);
            _mailService.SendAsync("Telldus Failure", Msg, Msg)
                .GetAwaiter().GetResult();
            throw;
        }
    }

    public async Task CheckBatteryAsync()
    {
        try
        {
            var sensorResponse = await _telldusClient.Sensors.GetSensorsAsync(false, true, false, true);
            var lowBatSensors = GetLowBatSensors(sensorResponse);
            await HandleBatteryLevels(lowBatSensors, sensorResponse);
            _logger.LogInformation("Battery status checked");
        }
        catch (Exception e)
        {
            const string Msg = "Failed to check battery level";
            await _mailService.SendAsync( "Telldus Failure", Msg, Msg);
            _logger.LogError(e, Msg);
        }
    }

    private async Task HandleBatteryLevels(
        List<Sensor> lowBatSensors,
        SensorsResponse sensorResponse)
    {
        if (lowBatSensors == null)
        {
            return;
        }
        if (lowBatSensors.Any())
        {
            var msg = string.Empty;
            
            foreach (var sensor in lowBatSensors)
            {
                msg += $"Sensor {sensor.Name} has low battery ({sensor.Battery}%) {Environment.NewLine}";
            }

            _logger.LogWarning(msg);
            var mailResponse = await _mailService.SendAsync("Telldus Warning", msg, msg);

            if (!mailResponse.IsSuccessStatusCode)
            {
                var error = mailResponse.Body
                    .ReadAsStringAsync().GetAwaiter().GetResult();
                _logger.LogError("Failed to send mail. Error: {error}", error);
            }
        }
        else
        {
            var count = sensorResponse.Sensors.Count;
            _logger.LogInformation("All {count} sensors have enough power", count);
        }
    }

    private static List<Sensor> GetLowBatSensors(SensorsResponse sensorResponse)
    {
        return sensorResponse.Sensors
            .Where(x =>
                x.Battery != (int)BatteryLevel.OutletPowered &&
                x.Battery < (int)BatteryLevel.Low)
            .ToList();
    }
}