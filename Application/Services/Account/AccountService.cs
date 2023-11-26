using Microsoft.AspNetCore.Identity;
using Domain.Models;
using Domain.Interfaces.Identity;
using Domain.Interfaces.Infra.Data;
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

        /// <summary>
        /// Obtém informações do usuário com base no <paramref name="user"/>.
        /// </summary>
        /// <param name="user">O usuário atualmente logado.</param>
        /// <returns>Uma tupla contendo um <see cref="OperationResultModel"/> e um <see cref="InfoUserModel"/>.</returns>
        public async Task<(OperationResultModel, InfoUserModel)> GetInfoUserAsync(ClaimsPrincipal user)
        {
            try
            {
                if (user == null)
                {
                    return (new OperationResultModel(false, "Nenhum dado recebido."), new InfoUserModel());
                }

                var User = _userManager.GetUserId(user);

                if (User == null)
                {
                    return (new OperationResultModel(false, "Nenhum Id encontrado."), new InfoUserModel());
                }

                var currUser = await _userManager.FindByIdAsync(User);

                if (currUser == null)
                {
                    return (new OperationResultModel(false, "Usuário não encontrado."), new InfoUserModel());
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

                return (new OperationResultModel(true, "Sucesso"), userModel);
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), new InfoUserModel());
            }
        }

        /// <summary>
        /// Atualiza as informações do usuário com base no <paramref name="model"/>.
        /// </summary>
        /// <param name="model">O modelo contendo as informações a serem atualizadas.</param>
        /// <param name="user">O usuário atualmente logado.</param>
        /// <returns>Um <see cref="OperationResultModel"/> indicando o resultado da operação.</returns>
        public async Task<OperationResultModel> UpdateInfoUserAsync(InfoUserModel model, ClaimsPrincipal user)
        {
            try
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
                    return new OperationResultModel(true, "Sucesso");
                }

                return new OperationResultModel(false, "Erro ao atualizar os dados");
            }
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Exceção não planejada: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica o e-mail do usuário com base no <paramref name="userId"/> e <paramref name="token"/>.
        /// </summary>
        /// <param name="userId">O ID do usuário.</param>
        /// <param name="token">O token de verificação do e-mail.</param>
        /// <returns>Um <see cref="OperationResultModel"/> indicando o resultado da operação.</returns>
        public async Task<OperationResultModel> VerifyEmailAsync(string userId, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
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

                return new OperationResultModel(false, "Erro ao confirmar o email, tente novamente ou entre em contato com o administrador.");
            }
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Exceção não planejada: {ex.Message}");
            }
        }

        /// <summary>
        /// Cria um novo usuário com o email especificado e a senha fornecida.
        /// </summary>
        /// <param name="userEmail">O email do usuário.</param>
        /// <param name="password">A senha do usuário.</param>
        /// <returns>Uma tupla contendo um <see cref="OperationResultModel"/> indicando o resultado da operação e um <see cref="ApplicationUser"/>.</returns>
        public async Task<(OperationResultModel, ApplicationUser user)> CreateUserAsync(string userEmail, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userEmail) || string.IsNullOrWhiteSpace(password))
                {
                    return (new OperationResultModel(false, "Nenhum dado recebido."), new ApplicationUser());
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
                var errors = result.Errors.Select(e => e.Description);
                var errorMessage = string.Join(Environment.NewLine, errors);

                return (new OperationResultModel(false, errorMessage), new ApplicationUser());
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), new ApplicationUser());
            }
        }

        /// <summary>
        /// Obtém o email do usuário atual com base no objeto <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="user">O objeto <see cref="ClaimsPrincipal"/> do usuário.</param>
        /// <returns>Uma tupla contendo um <see cref="OperationResultModel"/> indicando o resultado da operação e uma string representando o email do usuário.</returns>
        public async Task<(OperationResultModel, string userEmail)> GetUserEmailAsync(ClaimsPrincipal user)
        {
            try
            {
                if (user == null)
                {
                    return (new OperationResultModel(false, "Nenhum dado recebido."), string.Empty);
                }

                var currUser = await _userManager.GetUserAsync(user);

                if (currUser == null)
                {
                    return (new OperationResultModel(false, "Usuário não encontrado."), string.Empty);
                }

                return (new OperationResultModel(true, "Successo"), currUser.Email);
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), string.Empty);
            }
        }

        /// <summary>
        /// Verifica se o email do usuário atual foi confirmado.
        /// </summary>
        /// <param name="user">O objeto <see cref="ClaimsPrincipal"/> do usuário.</param>
        /// <returns>Um valor booleano indicando se o email do usuário foi confirmado.</returns>
        public async Task<bool> IsEmailConfirmedAsync(ClaimsPrincipal user)
        {
            try
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
            catch (Exception ex)
            {
                //Console.WriteLine($"Exceção não planejada: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica se o token de redefinição de senha do usuário foi usado.
        /// </summary>
        /// <param name="userId">O identificador exclusivo do usuário.</param>
        /// <returns>Um objeto <see cref="OperationResultModel"/> indicando se o token foi usado.</returns>
        public async Task<OperationResultModel> CheckIfTokenResetPasswordIsUsedAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
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
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Exceção não planejada: {ex.Message}");
            }
        }

        /// <summary>
        /// Envia um código de redefinição de senha para o email especificado.
        /// </summary>
        /// <param name="userEmail">O endereço de email do usuário para o qual o código será enviado.</param>
        /// <returns>Um objeto <see cref="OperationResultModel"/> que indica se o email foi enviado com sucesso.</returns>
        public async Task<OperationResultModel> SendCodeAsync(string userEmail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userEmail))
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
               
                return new OperationResultModel(false, "Houve algum erro entre a camada de service e de infraestrutura.");               
            }
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Exceção não planejada: {ex.Message}");
            }
        }

        /// <summary>
        /// Redefine a senha do usuário com o token fornecido e a nova senha especificada.
        /// </summary>
        /// <param name="userId">O ID do usuário cuja senha será redefinida.</param>
        /// <param name="token">O token usado para redefinir a senha.</param>
        /// <param name="newPassword">A nova senha a ser definida para o usuário.</param>
        /// <returns>Um objeto <see cref="OperationResultModel"/> que indica se a redefinição de senha foi bem-sucedida.</returns>
        public async Task<OperationResultModel> ResetPasswordAsync(string userId, string token, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
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
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Exceção não planejada: {ex.Message}");
            }
        }

        /// <summary>
        /// Envia um email de confirmação para o usuário com o endereço de email fornecido.
        /// </summary>
        /// <param name="userEmail">O endereço de email do usuário.</param>
        /// <returns>Um objeto <see cref="OperationResultModel"/> que indica se o email de confirmação foi enviado com sucesso.</returns>
        public async Task<OperationResultModel> ConfirmEmailAsync(string userEmail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userEmail))
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

                await _userManager.UpdateAsync(user);
                var emailSent = await _sendEmail.SendEmailAsync(model);
                return new OperationResultModel(true, "Successo.");
            }
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Exceção não planejada: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualiza a senha do usuário com a nova senha fornecida, desde que a senha atual também seja fornecida corretamente.
        /// </summary>
        /// <param name="newPassword">A nova senha desejada para o usuário.</param>
        /// <param name="currPassword">A senha atual do usuário.</param>
        /// <param name="user">O principal de reivindicações do usuário.</param>
        /// <returns>Um objeto <see cref="OperationResultModel"/> que indica se a senha foi atualizada com sucesso ou se houve uma falha.</returns>
        public async Task<OperationResultModel> UpdatePasswordAsync(string newPassword, string currPassword, ClaimsPrincipal user)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(currPassword) || user == null)
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
                var passwordCorrect = await _userManager.CheckPasswordAsync(currUser, currPassword);

                if (!passwordCorrect)
                {
                    return new OperationResultModel(false, "A senha atual está incorreta. Verifique e tente novamente.");
                }

                var result = await _userManager.ChangePasswordAsync(currUser, currPassword, newPassword);

                if (result.Succeeded)
                {
                    return new OperationResultModel(true, "Senha atualizada com sucesso.");
                }

                return new OperationResultModel(false, "Falha ao atualizar a senha.");
            }
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Exceção não planejada: {ex.Message}");
            }
        }


        //----------------------------------------------------------------------------------------------------------------------------------------
    }
}
