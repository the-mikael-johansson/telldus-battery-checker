using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using SensorBatteryChecker.Configurations;

namespace SensorBatteryChecker.Services.Mail;

/// <summary>
/// Setup sender at https://app.sendgrid.com/settings/sender_auth/senders
/// </summary>
public class SendGridService : IMailService
{
    private readonly SendGridConfiguration _configuration;

    public SendGridService(IOptions<SendGridConfiguration> options)
    {
        _configuration = options.Value;
    }

    public async Task<Response> SendAsync(
        string subject, 
        string plainTextContent,
        string htmlContent)
    {
        if (!_configuration.Enabled)
        {
            return null;
        }

        var client = new SendGridClient(_configuration.ApiKey);
        var from = new EmailAddress(_configuration.FromAddress, _configuration.FromName);
        var to = new EmailAddress(_configuration.ToAddress, _configuration.ToName);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg);
        return response;
    }
}