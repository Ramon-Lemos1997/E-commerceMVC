﻿using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.Extensions.Configuration;
using Domain.Models;
using Domain.Interfaces.Infra.Data;

namespace Infra.Data.SendEmail

{
    public class EmailService : ISendEmail
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //___________________________________________________________________


        /// <summary>
        /// Envia um e-mail usando a API SendGrid.
        /// </summary>
        /// <param name="model">O modelo de e-mail a ser enviado.</param>
        /// <returns>Verdadeiro se o e-mail for enviado com sucesso, caso contrário, falso.</returns>
        public async Task<bool> SendEmailAsync(SendEmailModel model)
        {
            try
            {
                var apiKey = _configuration["SendGridSettings:Key"];
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("lemosramonteste1997@gmail.com", "Ramon Lemos");
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
