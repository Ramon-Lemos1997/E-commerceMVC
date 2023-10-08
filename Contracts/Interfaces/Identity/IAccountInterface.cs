using Contracts.Models;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Contracts.Interfaces.Identity
{
    public interface IAccountInterface
    {
   
        Task<OperationResultModel> VerifyEmailAsync(string userId, string token);
        Task<(ApplicationUser user, IdentityResult result)> CreateUserAsync(string userEmail, string password);
        Task<(OperationResultModel, string userEmail)> GetUserEmailAsync(ClaimsPrincipal user);
        Task<bool> IsEmailConfirmedAsync(ClaimsPrincipal user);
        Task<OperationResultModel> CheckIfTokenResetPasswordIsUsedAsync(string userId);
        Task<OperationResultModel> SendCodeAsync(string model);
        Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword);      
        Task<OperationResultModel> ConfirmEmailAsync( string email);
        Task<(OperationResultModel, InfoUserModel)> GetInfoUserAsync(ClaimsPrincipal user);
        Task<OperationResultModel> UpdateInfoUserAsync(InfoUserModel model, ClaimsPrincipal user);
        Task<IdentityResult> UpdatePasswordAsync(string newPassowrd, string currPassword, ClaimsPrincipal user);



    }
}
