using Microsoft.AspNetCore.Identity;
using Contracts.Models;
using Contracts.Interfaces;


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
        

        public async Task<(bool success, string errorMessage)> Email(string userEmail)
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
           
//----------------------------------------------------------------------------------------------------------------------------------------
    }
}
