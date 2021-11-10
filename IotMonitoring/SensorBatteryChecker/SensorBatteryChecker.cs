using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Wolfberry.TelldusLive;
using Wolfberry.TelldusLive.Models.Sensor;
using static System.DateTime;

namespace SensorBatteryChecker
{
    public static class SensorBatteryChecker
    {
        /// <summary>
        /// "0 * * * * *" = every minute, "0 0 * * * *" = every hour
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="logger"></param>
        [FunctionName("SensorBatteryChecker")]
        public static async Task Run(
            [TimerTrigger("%TimeTrigger%")] TimerInfo timer,
            ILogger logger)
        {
            if (timer.IsPastDue)
            {
                logger.LogInformation("This run is a little bit late");
            }

            var sendgridConfiguration = new SendGridConfiguration();
            ITelldusLiveClient telldusClient;
                
            try
            {
                telldusClient = CreateTelldusLiveClient();
            }
            catch (Exception e)
            {
                const string msg = "Failed to initialize Telldus client. Check configuration";
                logger.LogError(e, msg);
                await SendGridMailer.Send(sendgridConfiguration, "Telldus Failure", msg, msg);
                return;
            }

            try
            {
                var sensorResponse = await telldusClient.Sensors.GetSensorsAsync(false, true, false, true);
                var lowBatSensors = GetLowBatSensors(sensorResponse);
                await HandleBatteryLevels(logger, sendgridConfiguration, lowBatSensors, sensorResponse);
            }
            catch (Exception e)
            {
                const string msg = "Failed to check battery level";
                await SendGridMailer.Send(sendgridConfiguration, "Telldus Failure", msg, msg);
                logger.LogError(e, msg);
                return;
            }

            logger.LogInformation($"Check finished {Now}");
        }

        private static async Task HandleBatteryLevels(
            ILogger log, 
            SendGridConfiguration sendGridConfiguration,
            List<Sensor> lowBatSensors, 
            SensorsResponse sensorResponse)
        {
            if (lowBatSensors == null)
            {
                return;
            }
            if (lowBatSensors.Any())
            {
                foreach (var sensor in lowBatSensors)
                {
                    var msg = $"Sensor {sensor.Name} has low battery ({sensor.Battery}%)";
                    log.LogWarning(msg);
                    await SendGridMailer.Send(sendGridConfiguration, "Telldus Warning", msg, msg);

                }
            }
            else
            {
                log.LogInformation($"All {sensorResponse.Sensors.Count} sensors have enough power");
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

        private static ITelldusLiveClient CreateTelldusLiveClient()
        {
            var consumerKey = Environment.GetEnvironmentVariable("ConsumerKey");
            var consumerKeySecret = Environment.GetEnvironmentVariable("ConsumerKeySecret");
            var accessToken = Environment.GetEnvironmentVariable("AccessToken");
            var accessTokenSecret = Environment.GetEnvironmentVariable("AccessTokenSecret");

            var client = new TelldusLiveClient(
                consumerKey, consumerKeySecret, accessToken, accessTokenSecret);

            return client;
        }
    }
}
