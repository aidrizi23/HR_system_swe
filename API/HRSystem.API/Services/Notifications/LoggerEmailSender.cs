using Microsoft.Extensions.Logging;

namespace HRSystem.API.Services.Notifications;

public class LoggerEmailSender : IEmailSender
{
    private readonly ILogger<LoggerEmailSender> _logger;

    public LoggerEmailSender(ILogger<LoggerEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toEmail, string subject, string body)
    {
        _logger.LogInformation("Email → {ToEmail} · subject={Subject}\n{Body}", toEmail, subject, body);
        return Task.CompletedTask;
    }
}
