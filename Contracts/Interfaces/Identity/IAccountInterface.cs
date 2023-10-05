using Contracts.Models;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Contracts.Interfaces.Identity
{
    public interface IAccountInterface
    {
        Task<(ApplicationUser user, IdentityResult result)> CreateUserAsync(string userName, string password);
        Task<(bool success, string errorMessage, string userEmail)> GetUserEmailAsync(ClaimsPrincipal user);
        Task<bool> IsEmailConfirmedAsync(ClaimsPrincipal user);
        Task<(bool success, string errorMessage)> CheckIfTokenResetPasswordIsUsedAsync(string userId);
        Task<(bool success, string errorMessage)> SendCode(string model);
        Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword);      
        Task<(bool success, string errorMessage)> ConfirmEmailAsync( string email);
   

    }
}
