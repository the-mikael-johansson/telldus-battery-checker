using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SensorBatteryChecker
{
    public class SendGridMailer
    {
        public static async Task<Response> Send(
            SendGridConfiguration configuration,
            string subject, 
            string plainTextContent,
            string htmlContent)
        {
            if (!configuration.Enabled)
            {
                return null;
            }
            var client = new SendGridClient(configuration.ApiKey);
            var from = new EmailAddress(configuration.FromName, configuration.FromAddress);
            var to = new EmailAddress(configuration.ToAddress, configuration.ToName);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            return response;
        }
    }
}