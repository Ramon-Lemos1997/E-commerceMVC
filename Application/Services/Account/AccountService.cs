using Microsoft.AspNetCore.Identity;
using Contracts.Models;
using Contracts.Interfaces.Identity;
using Contracts.Interfaces.Infra.Data;
using System.Security.Claims;
using Domain.Entities;

namespace Application.Services.Account
{
    public class AccountService : IAccountInterface
    {
        private readonly ISendEmail _sendEmail;
        private readonly UserManager<ApplicationUser> _userManager;
        public AccountService(ISendEmail sendEmail, UserManager<ApplicationUser> userManager)
        {
            _sendEmail = sendEmail;
            _userManager = userManager;        
        }

        //______________________________________________________________________________________

        public async Task<OperationResultModel> VerifyEmailAsync(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return new OperationResultModel(false, "Erro ao validar sua chave, tente novamente.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new OperationResultModel(false, "Usuário não encontrado.");
            }

            if (user.EmailConfirmed)
            {
                return new OperationResultModel(false, "Este link já foi usado.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return new OperationResultModel(true, "Email confirmado");
            }
            else
            {
                return new OperationResultModel(false, "Erro ao confirmar o email, tente novamente ou entre em contato com o administrador.");
            }
        }

        public async Task<(ApplicationUser user, IdentityResult result)> CreateUserAsync(string userName, string password)
        {
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = userName
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                var currDateClaim = new Claim("CadastradoEm", DateTime.Now.ToString());
                await _userManager.AddClaimAsync(user, currDateClaim);
                


                return (user, result);
            }

            return (null, result);
        }

        public async Task<(OperationResultModel, string userEmail)> GetUserEmailAsync(ClaimsPrincipal user)
        {
            var currUser = await _userManager.GetUserAsync(user);

            if (currUser == null)
            {
                return (new OperationResultModel(false, "Usuário não encontrado."), null);
            }

            return (new OperationResultModel(true, "Successo"), currUser.Email);
        }

        public async Task<bool> IsEmailConfirmedAsync(ClaimsPrincipal user)
        {
            var currUser = await _userManager.GetUserAsync(user);

            if (currUser == null)
            {
                return false;
            }

            return await _userManager.IsEmailConfirmedAsync(currUser);
        }

        public async Task<OperationResultModel> CheckIfTokenResetPasswordIsUsedAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);         
     
            if (user.ResetPassword)
            {
                return new OperationResultModel(false, "Este link já foi usado.");             
            }

            return new OperationResultModel(true, "Sucesso.");
        }

        public async Task<OperationResultModel> SendCode(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return new OperationResultModel(false, "Usuário não encontrado.");
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
                    if (user.ResetPassword)
                    {
                        user.ResetPassword = false;
                        await _userManager.UpdateAsync(user);                                        
                    }
                    return new OperationResultModel(true, "Email enviado com sucesso.");
                }
                else
                {
                    return new OperationResultModel(false, "Houve algum erro entre a camada de service e de infraestrutura.");
                }
            }
            catch
            {
                return new OperationResultModel(false, "Erro ao enviar o e-mail para a camada de infraestrutura.");
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
                user.ResetPassword = true;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }

        public async Task<OperationResultModel> ConfirmEmailAsync(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null || user.EmailConfirmed)
            {
                return new OperationResultModel(false, "Usuário não encontrado.");
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var scheme = "https";
            var host = "localhost:7034";
            var link = $"{scheme}://{host}/Account/EmailVerificado?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";

            var subject = "Confirmação de Conta";
            var message = $"Clique <a href=\"{link}\">aqui</a> para confirmar sua conta.";
            var model = new SendEmailModel
            {
                ToEmail = user.Email,
                Subject = subject,
                Message = message
            };
            try
            {             
                await _userManager.UpdateAsync(user);
                var emailSent = await _sendEmail.SendEmailAsync(model);                        
            }
            catch 
            {
                return new OperationResultModel(false, "Erro ao enviar o e-mail para a camada de infraestrutura.");
            }
            return new OperationResultModel(true, "Successo.");
        }

        //----------------------------------------------------------------------------------------------------------------------------------------
    }
}
