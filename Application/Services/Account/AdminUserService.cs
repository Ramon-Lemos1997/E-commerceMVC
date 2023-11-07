using Domain.Interfaces.Identity;
using Domain.Models;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace Application.Services.Account
{
    public class AdminUserService : IAdminUserInterface
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public AdminUserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Obtém a lista de todos os usuários.
        /// </summary>
        /// <returns>
        /// Uma tupla contendo um objeto <see cref="OperationResultModel"/> indicando se a operação foi bem-sucedida
        /// e uma lista de objetos <see cref="ApplicationUser"/> representando os usuários encontrados.
        /// </returns>
        public async Task<(OperationResultModel, List<ApplicationUser>)> GetUsersAsync()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();

                if (users == null || users.Count == 0)
                {
                    return (new OperationResultModel(false, "Nenhum usuário encontrado."), null);
                }

                return (new OperationResultModel(true, "Usuários encontrados com sucesso."), users);
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), null);
            }
        }

        ///não estou utilizando
        //public async Task<(OperationResultModel, ApplicationUser)> FindUserByIdAsync(string userId)
        //{
        //    if (userId == null)
        //    {
        //        return (new OperationResultModel(false, "Nenhum dado recebio."), null);
        //    }

        //    var user = await _userManager.FindByIdAsync(userId);

        //    if (user != null)
        //    {
        //        return (new OperationResultModel(true, "Usuário encontrado com sucesso."), user);

        //    }
        //    return (new OperationResultModel(false, "Usuário não encontrado."), null);
        //}

        /// <summary>
        /// Exclui um usuário com base no ID do usuário.
        /// </summary>
        /// <param name="userId">O ID do usuário que deve ser excluído.</param>
        /// <returns>
        /// Um objeto <see cref="OperationResultModel"/> indicando se a operação foi bem-sucedida.
        /// </returns>
        public async Task<OperationResultModel> DeleteUserAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return new OperationResultModel(false, "Nenhum dado recebido.");
                }

                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return new OperationResultModel(false, "Usuário não encontrado.");
                }

                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                if (isAdmin)
                {
                    return new OperationResultModel(false, "Você não tem permissão para excluir este usuário.");
                }

                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    return new OperationResultModel(true, "Usuário excluído com sucesso");
                }

                return new OperationResultModel(false, "Falha ao excluir o usuário");
            }
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Exceção não planejada: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém informações do usuário com base no ID do usuário.
        /// </summary>
        /// <param name="userId">O ID do usuário cujas informações devem ser recuperadas.</param>
        /// <returns>
        /// Uma tupla contendo um objeto <see cref="OperationResultModel"/> indicando se a operação foi bem-sucedida e um objeto 
        /// <see cref="InfoUserForAdminModel"/> contendo as informações do usuário.
        /// </returns>
        public async Task<(OperationResultModel, InfoUserForAdminModel)> GetInfoUserByIdAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return (new OperationResultModel(false, "Nenhum dado recebido."), null);
                }

                var currUser = await _userManager.FindByIdAsync(userId);

                if (currUser == null)
                {
                    return (new OperationResultModel(false, "Usuário não encontrado."), null);
                }

                var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                var userRole = await _userManager.GetRolesAsync(currUser);


                var userModel = new InfoUserForAdminModel
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
                    UserRole = userRole.FirstOrDefault(), //uso o first pois quero pegar a primeira role do user, na minha aplicação quero que cada user tenha apenas uma role
                    Roles = allRoles,
                    Id = currUser.Id,
                    AccountConfirmed = currUser.EmailConfirmed,
                };

                return (new OperationResultModel(true, "Sucesso"), userModel);
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), null);
            }
        }

        /// <summary>
        /// Atualiza o privilégio de um usuário com base no ID do usuário e no privilégio selecionado.
        /// </summary>
        /// <param name="userId">O ID do usuário cujo privilégio será atualizado.</param>
        /// <param name="selectedRole">O privilégio selecionado para ser atribuído ao usuário.</param>
        /// <returns>
        /// Um objeto <see cref="OperationResultModel"/> que indica se a operação de atualização foi bem-sucedida ou não.
        /// </returns>
        public async Task<OperationResultModel> UpdateRoleUserAsync(string userId, string selectedRole)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(selectedRole))
                {
                    return new OperationResultModel(false, "Nenhum dado recebido.");
                }

                var roleExists = await _roleManager.RoleExistsAsync(selectedRole);

                if (!roleExists)
                {
                    return new OperationResultModel(false, "O privilégio recebido não existe.");
                }

                var userForUpdate = await _userManager.FindByIdAsync(userId);

                if (userForUpdate == null)
                {
                    return new OperationResultModel(false, "Usuário não encontrado.");
                }

                var userRole = await _userManager.GetRolesAsync(userForUpdate);
                var currRoleForUpdate = userRole.FirstOrDefault(); // uso o first pois cada user tem apenas uma role

                //verifico se quem estou atualizando é admin
                var isAdmin = await _userManager.IsInRoleAsync(userForUpdate, "Admin");

                if (isAdmin)
                {
                    return new OperationResultModel(false, "Você não tem permissão para redefinir o privilégio deste usuário.");
                }

                //removo todas as roles do usuário, pois não existe um método de atualização
                if (currRoleForUpdate != null)
                {
                    await _userManager.RemoveFromRoleAsync(userForUpdate, currRoleForUpdate);
                }

                await _userManager.AddToRoleAsync(userForUpdate, selectedRole);
                return new OperationResultModel(true, "Privilégio redefinido com sucesso.");
            }
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Exceção não planejada: {ex.Message}");
            }
        }



        //-------------------------------------------------------------------------------------------------------------------------------------


    }
}
