using System;

namespace SensorBatteryChecker
{
    public class SendGridConfiguration
    {
        public string ApiKey { get; set; }
        public string FromName { get; set; }
        public string FromAddress { get; set; }
        public string ToName { get; set; }
        public string ToAddress { get; set; }

        public SendGridConfiguration()
        {
            ApiKey = Environment.GetEnvironmentVariable("SendGrid_ApiKey");
            FromName = Environment.GetEnvironmentVariable("SendGrid_FromName");
            FromAddress = Environment.GetEnvironmentVariable("SendGrid_FromAddress");
            ToName = Environment.GetEnvironmentVariable("SendGrid_ToName");
            ToAddress = Environment.GetEnvironmentVariable("SendGrid_ToAddress");
        }
    }
}