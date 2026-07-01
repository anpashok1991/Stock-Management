using Microsoft.Extensions.Logging;
using StockManagement.Application.Common.Interfaces;

namespace StockManagement.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger) => _logger = logger;

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        _logger.LogInformation("Email sent to {To} with subject {Subject} (stub — configure SMTP provider)", to, subject);
        return Task.CompletedTask;
    }
}
