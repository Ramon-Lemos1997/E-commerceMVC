using Contracts.Interfaces.Identity;
using Contracts.Interfaces.Infra.Data;
using Contracts.Models;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Presentation.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Account
{
    public class AdminRoleService : IAdminRoleInterface
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public AdminRoleService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------

        public async Task<OperationResultModel> CreateRoleAsync(string roleName)
        {
            if (roleName == null)
            {
                return new OperationResultModel(false, "Nenhum cargo recebido.");
            }
        
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

            if (result.Succeeded)
            {
                return new OperationResultModel(true, "Cargo criada com sucesso.");
            }

            return new OperationResultModel(false, "Falha ao criar cargo.");
        }

        public async Task<OperationResultModel> DeleteRoleByIdAsync(string roleId)
        {
            if (roleId == null )
            {
                return new OperationResultModel(false, "Nenhum dado recebio.");
            }

            var findRole = await _roleManager.FindByIdAsync(roleId);

            if (findRole == null)
            {
                return new OperationResultModel(false, "Cargo não encontrada.");
            }

            var deleteRole = await _roleManager.DeleteAsync(findRole);

            if (deleteRole.Succeeded)
            {
                return new OperationResultModel(true, "Cargo excluído com sucesso.");
            }

            return new OperationResultModel(false, "falha ao  excluir cargo.");
        }

        public async Task<(OperationResultModel, IdentityRole role)> GetRoleNameByIdAsync(string roleId)
        {
            if (roleId == null)
            {
                return (new OperationResultModel(false, "Nenhum dado recebio."), null);
            }

            var role = await _roleManager.FindByIdAsync(roleId);

            if (role != null)
            {
                return (new OperationResultModel(true, "Nome do cargo obtido com sucesso."), role);
               
            }

            return (new OperationResultModel(false, "Falha ao recuperar o nome do cargo."), null);
        }


        //______________________________________________________________________________________________________________________________________
    }
}
