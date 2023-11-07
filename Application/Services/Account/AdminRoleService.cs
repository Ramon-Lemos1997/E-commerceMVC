using Domain.Interfaces.Identity;
using Domain.Models;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Account
{
    public class AdminRoleService : IAdminRoleInterface
    {
        private RoleManager<IdentityRole> _roleManager;
        private UserManager<ApplicationUser> _userManager;
        public AdminRoleService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Cria uma função com o nome fornecido.
        /// </summary>
        /// <param name="roleName">O nome da função a ser criada.</param>
        /// <returns>
        /// Um objeto <see cref="OperationResultModel"/> que indica se a operação de criação foi bem-sucedida ou não.
        /// </returns>
        public async Task<OperationResultModel> CreateRoleAsync(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    return new OperationResultModel(false, "Nenhum cargo recebido.");
                }

                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

                if (result.Succeeded)
                {
                    return new OperationResultModel(true, "Cargo criado com sucesso.");
                }

                return new OperationResultModel(false, "Falha ao criar cargo.");
            }
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Exceção não planejada: {ex.Message}");
            }
        }

        /// <summary>
        /// Exclui a função com base no ID da função fornecido.
        /// </summary>
        /// <param name="roleId">O ID da função que deve ser excluída.</param>
        /// <returns>
        /// Um objeto <see cref="OperationResultModel"/> que indica se a operação de exclusão foi bem-sucedida ou não.
        /// </returns>
        public async Task<OperationResultModel> DeleteRoleByIdAsync(string roleId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleId))
                {
                    return new OperationResultModel(false, "Nenhum dado recebido.");
                }

                var findRole = await _roleManager.FindByIdAsync(roleId);

                if (findRole == null)
                {
                    return new OperationResultModel(false, "Privilégio não encontrado.");
                }

                if (findRole.Name == "Admin")
                {
                    return new OperationResultModel(false, "Você não tem permissão para excluir o privilégio de administrador.");
                }

                var deleteRole = await _roleManager.DeleteAsync(findRole);

                if (deleteRole.Succeeded)
                {
                    return new OperationResultModel(true, "Privilégio excluído com sucesso.");
                }

                return new OperationResultModel(false, "Falha ao excluir privilégio.");
            }
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Exceção não planejada: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém o nome da função com base no ID da função fornecido.
        /// </summary>
        /// <param name="roleId">O ID da função para a qual o nome deve ser obtido.</param>
        /// <returns>
        /// Uma tupla contendo um objeto <see cref="OperationResultModel"/> que indica se a operação foi bem-sucedida
        /// e a identidade da função correspondente ao ID fornecido, ou nulo se não houver dados disponíveis.
        /// </returns>
        public async Task<(OperationResultModel, IdentityRole role)> GetRoleNameByIdAsync(string roleId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleId))
                {
                    return (new OperationResultModel(false, "Nenhum dado recebido."), null);
                }

                var role = await _roleManager.FindByIdAsync(roleId);

                if (role != null)
                {
                    return (new OperationResultModel(true, "Nome do cargo obtido com sucesso."), role);

                }

                return (new OperationResultModel(false, "Falha ao recuperar o nome do cargo."), null);
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), null);
            }
        }

        /// <summary>
        /// Obtém os usuários associados a uma função específica com base no ID da função.
        /// </summary>
        /// <param name="roleId">O ID da função para a qual os usuários devem ser obtidos.</param>
        /// <returns>
        /// Uma tupla contendo um objeto <see cref="OperationResultModel"/> que indica se a operação foi bem-sucedida,
        /// o nome da função e uma coleção de usuários associados a essa função, ou nulo se não houver dados disponíveis.
        /// </returns>
        public async Task<(OperationResultModel, string roleName, IEnumerable<ApplicationUser>)> GetUsersOfRoleAsync(string roleId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleId))
                {
                    return (new OperationResultModel(false, "Nenhum dado recebido."), null, null);
                }
                var role = await _roleManager.FindByIdAsync(roleId);

                if (role != null)
                {
                    var usersOfRoles = await _userManager.GetUsersInRoleAsync(role.Name);
                    return (new OperationResultModel(true, "Usuários recuperados com sucesso."), role.Name, usersOfRoles);
                }

                return (new OperationResultModel(false, "Privilégio não encontrado."), null, null);
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), null, null);
            }
        }


        //______________________________________________________________________________________________________________________________________
    }
}
