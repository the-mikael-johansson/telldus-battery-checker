using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SensorBatteryChecker.Services.Battery;
using static System.DateTime;

namespace SensorBatteryChecker.Functions;

public class ScheduledBatteryChecker
{
    private readonly IBatteryService _batteryService;

    public ScheduledBatteryChecker(IBatteryService batteryService)
    {
        _batteryService = batteryService;
    }

    /// <summary>
    /// "0 * * * * *" = every minute, "0 0 * * * *" = every hour
    /// "0 */20 * * * *" = every 20 minute
    /// Do not poll more than each 15 minute
    /// </summary>
    /// <param name="timer"></param>
    /// <param name="logger"></param>
    [FunctionName("SensorBatteryChecker")]
    public async Task Run(
        [TimerTrigger("%TimeTrigger%"
#if DEBUG
            , RunOnStartup=true
#endif
        )] TimerInfo timer,
        ILogger logger)
    {
        if (timer.IsPastDue)
        {
            logger.LogInformation("Scheduled battery checker run little bit too late");
        }

        await _batteryService.CheckBatteryAsync();

        logger.LogInformation($"Check finished {Now}");
    }
}