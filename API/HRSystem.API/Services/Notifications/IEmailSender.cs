namespace HRSystem.API.Services.Notifications;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string body);
}
