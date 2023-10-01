using SendGrid.Helpers.Mail;
using SendGrid;
using E_commerce.MVC_ASP.NET.Services.Account;
using Domain.Interfaces;
using Domain.Models;

namespace E_commerce.MVC_ASP.NET.Infra.SendGrind

{
    public class SendEmailService : ISendEmail
    {
        private readonly IConfiguration _configuration;
        public SendEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(SendEmailModel model)
        {
            try
            {
                var apiKey = _configuration["SendGridSettings:Key"];
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("lemosramonteste1997@gmail.com", "Recuperação de conta");
                var to = new EmailAddress(model.ToEmail);
                var msg = MailHelper.CreateSingleEmail(from, to, model.Subject, plainTextContent: null, htmlContent: model.Message);

                var response = await client.SendEmailAsync(msg);
                return response.StatusCode == System.Net.HttpStatusCode.Accepted;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
