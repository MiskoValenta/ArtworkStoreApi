using System.Net;
using System.Net.Mail;
using ArtworkStoreApi.DTOs;

namespace ArtworkStoreApi.Utils
{
    public interface IEmailSender
{
    Task<bool> SendEmailAsync(string to, string subject, string body);
    Task<bool> SendWelcomeEmailAsync(string email);
    Task<bool> SendOrderConfirmationAsync(string email, OrderDto order);
}

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPass = _configuration["Email:SmtpPassword"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUser, smtpPass)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            
            mailMessage.To.Add(to);
            
            await client.SendMailAsync(mailMessage);
            _logger.LogInformation($"Email sent successfully to {to}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {to}");
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string email)
    {
        var subject = "Welcome to Art Gallery";
        var body = $@"
            <h1>Welcome!</h1>
            <p>Thank you for registering at our Art Gallery.</p>
            <p>Explore our beautiful collection of artworks.</p>";
            
        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendOrderConfirmationAsync(string email, OrderDto order)
    {
        var subject = $"Order Confirmation #{order.Id}";
        var body = $@"
            <h1>Order Confirmation</h1>
            <p>Your order #{order.Id} has been placed successfully.</p>
            <p>Total Amount: ${order.TotalAmount:F2}</p>
            <p>Status: {order.Status}</p>";
            
        return await SendEmailAsync(email, subject, body);
    }
}
}
