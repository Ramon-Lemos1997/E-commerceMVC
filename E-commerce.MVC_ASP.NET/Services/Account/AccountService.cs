
using SendGrid;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using System;
using E_commerce.MVC_ASP.NET.Models;
using SendGrid.Helpers.Mail.Model;
using Microsoft.AspNetCore.Mvc;
using E_commerce.MVC_ASP.NET.Domain.Interfaces;

namespace E_commerce.MVC_ASP.NET.Services.Account
{
    public class AccountService : IAccountInterface
    {
        private readonly ISendEmail _sendEmail;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountService(ISendEmail sendEmail, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _sendEmail = sendEmail;
            _userManager = userManager;
            _signInManager = signInManager;         
        }


        public async Task<(bool success, string errorMessage)> Email(string userId, string link)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {            
                return (false, "Ocorreu um erro ao enviar o e-mail.");
            }
                 
            var subject = "Redefinição de Senha";
            var message = $"Clique <a href=\"{link}\">aqui</a> para redefinir sua senha.";
            var model = new SendEmailModel
            {
                ToEmail = user.Email, 
                Subject = subject,   
                Message = message    
            };
            
            try
            {
                var emailSent = await _sendEmail.SendEmailAsync(model);
                if (emailSent)
                {
                    return (true, "Email enviado com sucesso.");
                }
                else
                {             
                    return (false, "Houve algum erro entre a camada de service e de infraestrutura.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao enviar o e-mail para a camada de infraestrutura.");
            }
        }
//----------------------------------------------------------------------------------------------------------------------------------------
    }
}
