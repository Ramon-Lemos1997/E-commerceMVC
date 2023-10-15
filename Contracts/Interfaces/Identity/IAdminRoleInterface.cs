using Contracts.Models;
using Microsoft.AspNetCore.Identity;

namespace Contracts.Interfaces.Identity
{
   public interface IAdminRoleInterface
    {
        Task<OperationResultModel> CreateRoleAsync(string roleName);     
        Task<OperationResultModel> DeleteRoleByIdAsync(string roleId);
        Task<(OperationResultModel, IdentityRole role)> GetRoleNameByIdAsync(string roleId);
    }
}
