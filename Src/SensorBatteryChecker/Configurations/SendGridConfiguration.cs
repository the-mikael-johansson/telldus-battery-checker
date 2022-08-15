namespace SensorBatteryChecker.Configurations;

public class SendGridConfiguration
{
    public bool Enabled { get; set; }
    public string ApiKey { get; set; }
    public string FromName { get; set; }
    public string FromAddress { get; set; }
    public string ToName { get; set; }
    public string ToAddress { get; set; }
}