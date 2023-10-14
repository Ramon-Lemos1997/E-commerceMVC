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
        public async Task<(OperationResultModel, InfoUserModel)> GetInfoUserAsync(ClaimsPrincipal user)
        {
            if (user == null)
            {
                return (new OperationResultModel(false, "Nenhum dado recebido."), null);
            }

            var User = _userManager.GetUserId(user);

            if (User == null)
            {
                return (new OperationResultModel(false, "Nenhum Id encontrado."), null);
            }

            var currUser = await _userManager.FindByIdAsync(User);

            if (currUser == null)
            {
                return (new OperationResultModel(false, "Usuário não encontrado."), null);
            }

            var userModel = new InfoUserModel
            {
                ZipCode = currUser.ZipCode,
                Street = currUser.Street,
                Neighborhood = currUser.Neighborhood,
                Gender = currUser.Gender,
                BirthDate = currUser.BirthDate,
                Email = currUser.Email,
                FirstName = currUser.FirstName,
                Surname = currUser.Surname,
                City = currUser.City,
                HouseNumber = currUser.HouseNumber,
                PhoneNumber = currUser.PhoneNumber,
                State = currUser.State,
                Nation = currUser.Nation,
            };

            return (new OperationResultModel(true, "Successo"), userModel);
        }

        public async Task<OperationResultModel> UpdateInfoUserAsync( InfoUserModel model, ClaimsPrincipal user)
        {
            if (user == null)
            {
                return new OperationResultModel(false, "Nenhum dado recebido.");
            }

            var User = _userManager.GetUserId(user);

            if (User == null)
            {
                return new OperationResultModel(false, "Nenhum Id encontrado.");
            }

            var currUser = await _userManager.FindByIdAsync(User);
                  
            if (currUser == null)
            {
                return new OperationResultModel(false, "Usuário não encontrado.");
            }

            if (currUser.Email != model.Email)
            {
                currUser.EmailConfirmed = false;             
            }
   
            currUser.UserName = model.Email;
            currUser.Nation = model.Nation;
            currUser.Email = model.Email;
            currUser.FirstName = model.FirstName;
            currUser.Surname = model.Surname;
            currUser.BirthDate = model.BirthDate;
            currUser.Gender = model.Gender;
            currUser.State = model.State;
            currUser.Street = model.Street;
            currUser.HouseNumber = model.HouseNumber;
            currUser.Neighborhood = model.Neighborhood;
            currUser.PhoneNumber = model.PhoneNumber;
            currUser.City = model.City; 
            currUser.ZipCode = model.ZipCode;

            var result = await _userManager.UpdateAsync(currUser);

            if (result.Succeeded)
            {
                return new OperationResultModel(true, "Successo");
            }

            return new OperationResultModel(false, "Erro ao atualizar os dados");
        }

        public async Task<OperationResultModel> VerifyEmailAsync(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return new OperationResultModel(false, "Erro ao validar seus dados, tente novamente.");
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

        public async Task<(OperationResultModel, ApplicationUser user)> CreateUserAsync(string userEmail, string password)
        {
            if (userEmail == null || password == null)
            {
                return (new OperationResultModel(false, "Nenhum dado recebido."), null);
            }
    
            // Obtém a data e hora atual no fuso horário do Brasil
            TimeZoneInfo brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            DateTime brazilDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brazilTimeZone);
            var currUser = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                CreationDate = brazilDateTime,
                BirthDate = DateTime.MinValue,
                FirstName = string.Empty,
                Surname = string.Empty, 
                Gender = string.Empty, 
                City = string.Empty, 
                Street = string.Empty, 
                Neighborhood = string.Empty, 
                ZipCode = string.Empty, 
                Nation = string.Empty, 
                HouseNumber = string.Empty, 
                State = string.Empty 
            };

            var result = await _userManager.CreateAsync(currUser, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(currUser, "User");
                //var currDateClaim = new Claim("CadastradoEm", DateTime.Now.ToString());
                //await _userManager.AddClaimAsync(user, currDateClaim);
                return (new OperationResultModel(true, "Usuário criado com sucesso"), currUser);
            }

            return (new OperationResultModel(false, "Falha ao criar o usuário"), null);
        }

        public async Task<(OperationResultModel, string userEmail)> GetUserEmailAsync(ClaimsPrincipal user)
        {
            if (user == null)
            {
                return (new OperationResultModel(false, "Nenhum dado recebido."), null);
            }
          
            var currUser = await _userManager.GetUserAsync(user);

            if (currUser == null)
            {
                return (new OperationResultModel(false, "Usuário não encontrado."), null);
            }

            return (new OperationResultModel(true, "Successo"), currUser.Email);
        }

        public async Task<bool> IsEmailConfirmedAsync(ClaimsPrincipal user)
        {
            if (user == null)
            {
                return false;
            }

            var currUser = await _userManager.GetUserAsync(user);

            if (currUser == null)
            {
                return false;
            }

            return await _userManager.IsEmailConfirmedAsync(currUser);
        }

        public async Task<OperationResultModel> CheckIfTokenResetPasswordIsUsedAsync(string userId)
        {
            if (userId == null)
            {
                return new OperationResultModel(false, "Usuário não encontrado");
            }
            var user = await _userManager.FindByIdAsync(userId);         
     
            if (user.ResetPassword)
            {
                return new OperationResultModel(false, "Este link já foi usado.");             
            }

            return new OperationResultModel(true, "Sucesso.");
        }

        public async Task<OperationResultModel> SendCodeAsync(string userEmail)
        {
            if (userEmail == null)
            {
                return new OperationResultModel(false, "Nenhum dado recebido.");
            }

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
       
        public async Task<OperationResultModel> ResetPasswordAsync(string userId, string token, string newPassword)
        {
            if (userId == null || token == null || newPassword == null)
            {
                return new OperationResultModel(false, "Nenhum dado recebido.");
            }
         
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new OperationResultModel(false, "Usuário não encontrado.");
  
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                user.ResetPassword = true;
                await _userManager.UpdateAsync(user);
                return new OperationResultModel(true, "Senha redefinida com sucesso.");
            }

            return new OperationResultModel(false, "Falha em redefinir senha.");
        }

        public async Task<OperationResultModel> ConfirmEmailAsync(string userEmail)
        {
            if (userEmail == null)
            {
                return new OperationResultModel(false, "Nenhum dado recebido.");
            }

            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null || user.EmailConfirmed)
            {
                return new OperationResultModel(false, "Usuário não encontrado.");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var scheme = "https";
            var host = "localhost:7034";
            var link = $"{scheme}://{host}/Account/VerifiedEmail?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";

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

        public async Task<OperationResultModel> UpdatePasswordAsync(string newPassword, string currPassword, ClaimsPrincipal user)
        {
            if (newPassword == null || currPassword == null || user == null)
            {
                return new OperationResultModel(false, "Nenhum dado recebido.");
            }

            var User = _userManager.GetUserId(user);

            if (User == null)
            {
                return new OperationResultModel(false, "Usuário não encontrado.");              
            }

            var currUser = await _userManager.FindByIdAsync(User);

            if (currUser == null)
            {
                return new OperationResultModel(false, "Usuário não encontrado.");
            }                

            var result = await _userManager.ChangePasswordAsync(currUser, currPassword, newPassword);
     
            if (result.Succeeded)
            {
                return new OperationResultModel(true, "Senha atualizada com sucesso.");
            }

            return new OperationResultModel(false, "Falha ao atualizar a senha.");
        }

        //----------------------------------------------------------------------------------------------------------------------------------------
    }
}
