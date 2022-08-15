using SensorBatteryChecker.Configurations;
using Wolfberry.TelldusLive;

namespace SensorBatteryChecker.Factories;

public static class TelldusClientFactory
{

    public static ITelldusLiveClient CreateTelldusLiveClient(
        TelldusConfiguration configuration)
    {
        var client = new TelldusLiveClient(
            configuration.ConsumerKey, 
            configuration.ConsumerKeySecret, 
            configuration.AccessToken, 
            configuration.AccessTokenSecret);

        return client;
    }
}