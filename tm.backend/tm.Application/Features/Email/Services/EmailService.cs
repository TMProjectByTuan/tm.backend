using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using tm.Application.Features.Email.Interfaces;

namespace tm.Application.Features.Email.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string fullName, CancellationToken cancellationToken = default)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
        var smtpUsername = emailSettings["SmtpUsername"] ?? "";
        var smtpPassword = emailSettings["SmtpPassword"] ?? "";
        var fromEmail = emailSettings["FromEmail"] ?? smtpUsername;
        var fromName = emailSettings["FromName"] ?? "TM Project Management";

        _logger.LogInformation("Attempting to send welcome email to {Email} using SMTP server {SmtpServer}:{SmtpPort}", 
            toEmail, smtpServer, smtpPort);
        _logger.LogInformation("SMTP Username: {SmtpUsername}, FromEmail: {FromEmail}", 
            smtpUsername, fromEmail);

        // Validate configuration
        if (string.IsNullOrEmpty(smtpUsername) || smtpUsername == "your-email@gmail.com")
        {
            _logger.LogError("Email configuration is invalid. SmtpUsername is not set correctly.");
            throw new InvalidOperationException("Email configuration is invalid. Please check appsettings.json");
        }

        if (string.IsNullOrEmpty(smtpPassword))
        {
            _logger.LogError("Email configuration is invalid. SmtpPassword is not set.");
            throw new InvalidOperationException("Email configuration is invalid. Please check appsettings.json");
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(fullName, toEmail));
            message.Subject = "Chào mừng đến với TM Project Management";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2 style='color: #2563eb;'>Chào mừng {fullName}!</h2>
                        <p>Cảm ơn bạn đã đăng ký tài khoản tại TM Project Management.</p>
                        <p>Tài khoản của bạn đã được tạo thành công với email: <strong>{toEmail}</strong></p>
                        <p>Bạn có thể bắt đầu tạo dự án và quản lý task ngay bây giờ.</p>
                        <br/>
                        <p>Trân trọng,<br/>Đội ngũ TM Project Management</p>
                    </body>
                    </html>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            _logger.LogInformation("Connecting to SMTP server {SmtpServer}:{SmtpPort}...", smtpServer, smtpPort);
            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls, cancellationToken);
            _logger.LogInformation("Connected to SMTP server successfully");

            _logger.LogInformation("Authenticating with SMTP server...");
            await client.AuthenticateAsync(smtpUsername, smtpPassword, cancellationToken);
            _logger.LogInformation("Authenticated successfully");

            _logger.LogInformation("Sending email to {Email}...", toEmail);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Welcome email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}. Error: {ErrorMessage}", 
                toEmail, ex.Message);
            // Throw exception để biết lỗi cụ thể
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }

    public async Task SendProjectInvitationAsync(string toEmail, string inviterName, string projectName, string invitationToken, CancellationToken cancellationToken = default)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
        var smtpUsername = emailSettings["SmtpUsername"] ?? "";
        var smtpPassword = emailSettings["SmtpPassword"] ?? "";
        var fromEmail = emailSettings["FromEmail"] ?? smtpUsername;
        var fromName = emailSettings["FromName"] ?? "TM Project Management";

        _logger.LogInformation("Attempting to send project invitation email to {Email} for project {ProjectName}", 
            toEmail, projectName);

        // Validate configuration
        if (string.IsNullOrEmpty(smtpUsername) || smtpUsername == "your-email@gmail.com")
        {
            _logger.LogError("Email configuration is invalid. SmtpUsername is not set correctly.");
            throw new InvalidOperationException("Email configuration is invalid. Please check appsettings.json");
        }

        if (string.IsNullOrEmpty(smtpPassword))
        {
            _logger.LogError("Email configuration is invalid. SmtpPassword is not set.");
            throw new InvalidOperationException("Email configuration is invalid. Please check appsettings.json");
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = $"Lời mời tham gia dự án: {projectName}";

            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:3000";
            var acceptLink = $"{baseUrl}/invitation/accept?token={Uri.EscapeDataString(invitationToken)}";
            var declineLink = $"{baseUrl}/invitation/decline?token={Uri.EscapeDataString(invitationToken)}";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; padding: 20px;'>
                        <div style='max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; padding: 30px;'>
                            <h2 style='color: #2563eb; margin-bottom: 20px;'>Lời mời tham gia dự án</h2>
                            <p style='font-size: 16px; line-height: 1.6;'>Xin chào,</p>
                            <p style='font-size: 16px; line-height: 1.6;'>
                                <strong>{inviterName}</strong> đã mời bạn tham gia dự án <strong style='color: #2563eb;'>{projectName}</strong>.
                            </p>
                            <p style='font-size: 16px; line-height: 1.6; margin-top: 30px;'>
                                Vui lòng click vào các nút bên dưới để chấp nhận hoặc từ chối lời mời:
                            </p>
                            <div style='margin: 30px 0; text-align: center;'>
                                <a href='{acceptLink}' 
                                   style='display: inline-block; background-color: #10b981; color: white; 
                                          padding: 12px 30px; text-decoration: none; border-radius: 6px; 
                                          margin-right: 10px; font-weight: bold;'>
                                    Chấp nhận
                                </a>
                                <a href='{declineLink}' 
                                   style='display: inline-block; background-color: #ef4444; color: white; 
                                          padding: 12px 30px; text-decoration: none; border-radius: 6px; 
                                          font-weight: bold;'>
                                    Từ chối
                                </a>
                            </div>
                            <p style='font-size: 14px; color: #666; margin-top: 30px;'>
                                Hoặc copy và paste link vào trình duyệt:<br/>
                                <span style='word-break: break-all;'>{acceptLink}</span>
                            </p>
                            <p style='font-size: 14px; color: #666; margin-top: 20px;'>
                                <strong>Lưu ý:</strong> Link này sẽ hết hạn sau 7 ngày. 
                                Nếu bạn chưa có tài khoản, hệ thống sẽ yêu cầu bạn đăng ký trước khi tham gia dự án.
                            </p>
                            <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 30px 0;'/>
                            <p style='font-size: 14px; color: #999; text-align: center;'>
                                Trân trọng,<br/>Đội ngũ TM Project Management
                            </p>
                        </div>
                    </body>
                    </html>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            _logger.LogInformation("Connecting to SMTP server {SmtpServer}:{SmtpPort}...", smtpServer, smtpPort);
            await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls, cancellationToken);
            _logger.LogInformation("Connected to SMTP server successfully");

            _logger.LogInformation("Authenticating with SMTP server...");
            await client.AuthenticateAsync(smtpUsername, smtpPassword, cancellationToken);
            _logger.LogInformation("Authenticated successfully");

            _logger.LogInformation("Sending invitation email to {Email}...", toEmail);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Project invitation email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send project invitation email to {Email}. Error: {ErrorMessage}", 
                toEmail, ex.Message);
            // Throw exception để biết lỗi cụ thể
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }
}

