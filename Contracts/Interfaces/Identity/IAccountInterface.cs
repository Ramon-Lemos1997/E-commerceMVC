using Contracts.Models;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Contracts.Interfaces.Identity
{
    public interface IAccountInterface
    {
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<OperationResultModel> VerifyEmailAsync(string userId, string token);
        Task<(ApplicationUser user, IdentityResult result)> CreateUserAsync(string userName, string password);
        Task<(OperationResultModel, string userEmail)> GetUserEmailAsync(ClaimsPrincipal user);
        Task<bool> IsEmailConfirmedAsync(ClaimsPrincipal user);
        Task<OperationResultModel> CheckIfTokenResetPasswordIsUsedAsync(string userId);
        Task<OperationResultModel> SendCode(string model);
        Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword);      
        Task<OperationResultModel> ConfirmEmailAsync( string email);
   

    }
}
