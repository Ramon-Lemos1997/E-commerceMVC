using Domain.Models;
using Domain.Entities;

namespace Domain.Interfaces.Identity
{
    public interface IAdminUserInterface
    {
        Task<(OperationResultModel, List<ApplicationUser>)> GetUsersAsync();
        //Task<(OperationResultModel, ApplicationUser)> FindUserByIdAsync(string userId);
        Task<OperationResultModel> DeleteUserAsync(string userId);
        Task<(OperationResultModel, InfoUserForAdminModel)> GetInfoUserByIdAsync(string userId);
        Task<OperationResultModel> UpdateRoleUserAsync(string userId, string selectedRole);
    }
}
