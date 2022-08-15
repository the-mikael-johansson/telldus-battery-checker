using System.Threading.Tasks;
using SendGrid;

namespace SensorBatteryChecker.Services.Mail;

public interface IMailService
{
    /// <summary>
    /// Send mail to pre-configured receiver
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="plainTextContent"></param>
    /// <param name="htmlContent"></param>
    /// <returns></returns>
    Task<Response> SendAsync(
        string subject,
        string plainTextContent,
        string htmlContent);
}