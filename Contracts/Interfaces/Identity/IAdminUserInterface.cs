using Contracts.Models;
using Domain.Entities;
using System.Security.Claims;

namespace Contracts.Interfaces.Identity
{
    public interface IAdminUserInterface
    {
        Task<(OperationResultModel, List<ApplicationUser>)> GetUsersAsync();
        //Task<(OperationResultModel, ApplicationUser)> FindUserByIdAsync(string userId);
        Task<OperationResultModel> DeleteUserAsync(string userId);
        Task<(OperationResultModel, InfoUserModel)> GetInfoUserByIdAsync(string userId);
        Task<OperationResultModel> UpdateRoleUserAsync(ClaimsPrincipal user, string userId, string selectedRole);
    }
}
