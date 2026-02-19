using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace SmartEduERP.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["Email:FromName"] ?? "SmartEduERP System",
                _config["Email:FromAddress"] ?? "noreply@smarteduerp.com"));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // For development, allow insecure connections
            await client.ConnectAsync(
                _config["Email:SmtpServer"] ?? "smtp.gmail.com",
                int.Parse(_config["Email:Port"] ?? "587"),
                MailKit.Security.SecureSocketOptions.StartTls);

            // Authenticate if credentials provided
            var username = _config["Email:Username"];
            var password = _config["Email:Password"];
            
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                await client.AuthenticateAsync(username, password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // Log error (in production, use proper logging)
            Console.WriteLine($"Email sending failed: {ex.Message}");
            throw;
        }
    }

    #region Email Templates

    public string GetWelcomeEmailTemplate(string firstName, string username)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #777; }}
        .button {{ background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; display: inline-block; border-radius: 5px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to SmartEduERP! 🎓</h1>
        </div>
        <div class='content'>
            <h2>Hello {firstName},</h2>
            <p>Your account has been successfully created in SmartEduERP System.</p>
            <p><strong>Username:</strong> {username}</p>
            <p>You can now login and access all the features available to you.</p>
            <p style='text-align: center; margin-top: 20px;'>
                <a href='https://localhost:5001' class='button'>Login Now</a>
            </p>
        </div>
        <div class='footer'>
            <p>&copy; 2025 SmartEduERP. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GetGradeNotificationTemplate(string studentName, string subjectName, string grade, string teacher)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #6c757d; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; }}
        .grade-box {{ background-color: #28a745; color: white; font-size: 24px; padding: 15px; text-align: center; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>New Grade Posted 🎯</h1>
        </div>
        <div class='content'>
            <h2>Hello {studentName},</h2>
            <p>A new grade has been posted for your subject:</p>
            <p><strong>Subject:</strong> {subjectName}</p>
            <p><strong>Teacher:</strong> {teacher}</p>
            <div class='grade-box'>
                Grade: {grade}
            </div>
            <p>Login to your dashboard to view more details.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2025 SmartEduERP. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GetPaymentConfirmationTemplate(string studentName, decimal amount, string paymentDate, string paymentMethod)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; }}
        .amount-box {{ background-color: #28a745; color: white; font-size: 28px; padding: 15px; text-align: center; border-radius: 5px; margin: 20px 0; }}
        .details {{ background-color: white; padding: 15px; border-left: 4px solid #dc3545; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Payment Confirmation 💰</h1>
        </div>
        <div class='content'>
            <h2>Hello {studentName},</h2>
            <p>Your payment has been successfully received and processed.</p>
            <div class='amount-box'>
                ₱{amount:N2}
            </div>
            <div class='details'>
                <p><strong>Payment Date:</strong> {paymentDate}</p>
                <p><strong>Payment Method:</strong> {paymentMethod}</p>
                <p><strong>Status:</strong> <span style='color: #28a745;'>Paid</span></p>
            </div>
            <p>Thank you for your payment. This is an automated confirmation email.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2025 SmartEduERP. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GetEnrollmentConfirmationTemplate(string studentName, string subjectName, string academicYear, string semester)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #ffc107; color: #333; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; }}
        .enrollment-box {{ background-color: white; padding: 15px; border-left: 4px solid #ffc107; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Enrollment Confirmed 📝</h1>
        </div>
        <div class='content'>
            <h2>Hello {studentName},</h2>
            <p>Your enrollment has been successfully processed.</p>
            <div class='enrollment-box'>
                <p><strong>Subject:</strong> {subjectName}</p>
                <p><strong>Academic Year:</strong> {academicYear}</p>
                <p><strong>Semester:</strong> {semester}</p>
            </div>
            <p>You can now access the subject materials and attend classes.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2025 SmartEduERP. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GetEmailConfirmationTemplate(string firstName, string confirmationUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #777; }}
        .button {{ background-color: #28a745; color: white; padding: 15px 30px; text-decoration: none; display: inline-block; border-radius: 5px; font-weight: bold; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Confirm Your Email Address 📧</h1>
        </div>
        <div class='content'>
            <h2>Hello {firstName},</h2>
            <p>Thank you for registering with SmartEduERP! To complete your registration and activate your student account, please confirm your email address by clicking the button below:</p>
            <p style='text-align: center; margin: 30px 0;'>
                <a href='{confirmationUrl}' class='button'>Confirm Email Address</a>
            </p>
            <div class='warning'>
                <p><strong>Important:</strong> You must confirm your email address to access your student dashboard and all system features.</p>
            </div>
            <p>If you didn't create this account, please ignore this email.</p>
            <p><small>This link will expire in 24 hours for security purposes.</small></p>
        </div>
        <div class='footer'>
            <p>&copy; 2025 SmartEduERP. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    #endregion
}
