using Contracts.Interfaces.Identity;
using Contracts.Models;
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

        //obter a lista de users
        public async Task<(OperationResultModel, List<ApplicationUser>)> GetUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();

            if (users == null || users.Count == 0)
            {
                return (new OperationResultModel(false, "Nenhum usuário encontrado."), null);
            }

            return (new OperationResultModel(true, "Usuários encontrados com sucesso."), users);
        }

        ////não estou utilizando
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

        public async Task<OperationResultModel> DeleteUserAsync(string userId)
        {
            if (userId == null)
            {
                return new OperationResultModel(false, "Nenhum dado recebio.");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new OperationResultModel(false, "Usuário não encontrado.");
            }
           
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return new OperationResultModel(true, "Usuário excluído com sucesso");
            }

            return new OperationResultModel(false, "Falha ao excluir o usuário");
            
        }

        //obter a info do user, utilizo tanto do detalhes, quanto na tela de redefinir o privilégio
        public async Task<(OperationResultModel, InfoUserModel)> GetInfoUserByIdAsync(string userId)
        {
            if (userId == null)
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
                UserRole = userRole.FirstOrDefault(), //uso o first pois quero pegar a primeira role do user, na minha aplicação quero que cada user tenha apenas uma role
                Roles = allRoles,
                Id = currUser.Id,
            };

            return (new OperationResultModel(true, "Successo"), userModel);
        }

        //recebo o user logado, o user que irá receber a modificação e a nova role, faço as verificações se existem e depois
        // verifico se possui a mesma role, se não possuirem permito a mudança, somente admin podem fazer esta alteração
        //utilizo o first para pega a role do user, cada user tem apenas uma
        public async Task<OperationResultModel> UpdateRoleUserAsync(ClaimsPrincipal user, string userId, string selectedRole)
        {
            if (user == null ||userId == null || selectedRole == null)
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
            var currRoleForUpdate = userRole.FirstOrDefault();

            //verifico se o user atual está tentando mudar o privilégio de outro user com a mesma role
            var loggedInUser = await _userManager.GetUserAsync(user);

            if (loggedInUser == null)
            {
                return new OperationResultModel(false, "Erro na operação.");
            }
            var userLoggedRole = await _userManager.GetRolesAsync(loggedInUser);
            var firstRoleOfUserLogged = userLoggedRole.FirstOrDefault();
            
            //usuários tem a mesma role
            if (firstRoleOfUserLogged == currRoleForUpdate)
            {
                return new OperationResultModel(false, "Você não tem permissão para alterar o privilégio deste usuário.");
            }
       
            //removo todas roles do user, pois não tem o método de update
            if (currRoleForUpdate != null)
            {
                await _userManager.RemoveFromRoleAsync(userForUpdate, currRoleForUpdate); 
            }

            await _userManager.AddToRoleAsync(userForUpdate, selectedRole);
            return new OperationResultModel(true, "Privilégio redefinido com sucesso.");
        }

        //-------------------------------------------------------------------------------------------------------------------------------------


    }
}
