using Microsoft.AspNetCore.Identity;
using Contracts.Models;
using Contracts.Interfaces.Identity;
using Contracts.Interfaces.Infra.Data;
using System.Security.Claims;
using Azure.Core;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Net;
using System;

namespace Application.Services.Account
{
    public class AccountService : IAccountInterface
    {
        private readonly ISendEmail _sendEmail;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountService(ISendEmail sendEmail, UserManager<IdentityUser> userManager)
        {
            _sendEmail = sendEmail;
            _userManager = userManager;
  
        }

        public async Task<(bool success, string errorMessage, string userEmail)> GetUserEmailAsync(ClaimsPrincipal user)
        {
            var currUser = await _userManager.GetUserAsync(user);

            if (currUser == null)
            {
                return (false, "Usuário não encontrado.", null);
            }

            return (true, "Success", currUser.Email);
        }

        public async Task<bool> IsEmailConfirmedAsync(ClaimsPrincipal user)
        {
            var currentUser = await _userManager.GetUserAsync(user);

            if (currentUser == null)
            {
                return false;
            }

            return await _userManager.IsEmailConfirmedAsync(currentUser);
        }
        public async Task<(bool success, string errorMessage)> CheckIfTokenResetPasswordIsUsedAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var tokenUsed = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "ResetPassword");

            if (tokenUsed != null)
            {
                return (false, "Este link já foi usado.");
            }

            return (true, "Success");
        }
        public async Task<(bool success, string errorMessage)> SendCode(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {            
                return (false, "Usuário não encontrado.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var scheme = "https";
            var host = "localhost:7034";
            var link = $"{scheme}://{host}/Account/ResetPassword?token={Uri.EscapeDataString(token)}&userId={Uri.EscapeDataString(user.Id)}";

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
                    var resetPasswordClaim = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "ResetPassword");
                    if (resetPasswordClaim != null)
                    {
                        await _userManager.RemoveClaimAsync(user, resetPasswordClaim);                      
                    }
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
       
        public async Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {               
                return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado." });
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                await _userManager.AddClaimAsync(user, new Claim("ResetPassword", "true"));
            }

            return result;
        }

        public async Task<(bool success, string errorMessage)> ConfirmEmailAsync(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null || user.EmailConfirmed)
            {
                return (false, "Usuário não encontrado.");
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var scheme = "https";
            var host = "localhost:7034";
            var link = $"{scheme}://{host}/Account/EmailVerificado?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";

            var subject = "Confirmação de Email";
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
                        
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao enviar o e-mail para a camada de infraestrutura.");
            }
            return (true, "Success.");
        }

        //----------------------------------------------------------------------------------------------------------------------------------------
    }
}
