using Contracts.Models;
using Domain.Entities;
using System.Security.Claims;

namespace Contracts.Interfaces.Identity
{
    public interface IAccountInterface
    {
   
        Task<OperationResultModel> VerifyEmailAsync(string userId, string token);
        Task<(OperationResultModel, ApplicationUser user)> CreateUserAsync(string userEmail, string password);
        Task<(OperationResultModel, string userEmail)> GetUserEmailAsync(ClaimsPrincipal user);
        Task<bool> IsEmailConfirmedAsync(ClaimsPrincipal user);
        Task<OperationResultModel> CheckIfTokenResetPasswordIsUsedAsync(string userId);
        Task<OperationResultModel> SendCodeAsync(string model);
        Task<OperationResultModel> ResetPasswordAsync(string userId, string token, string newPassword);      
        Task<OperationResultModel> ConfirmEmailAsync( string email);
        Task<(OperationResultModel, InfoUserModel)> GetInfoUserAsync(ClaimsPrincipal user);
        Task<OperationResultModel> UpdateInfoUserAsync(InfoUserModel model, ClaimsPrincipal user);
        Task<OperationResultModel> UpdatePasswordAsync(string newPassowrd, string currPassword, ClaimsPrincipal user);



    }
}
