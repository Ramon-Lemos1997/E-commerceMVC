﻿using Domain.Models;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Domain.Interfaces.Identity
{
   public interface IAdminRoleInterface
    {
        Task<OperationResultModel> CreateRoleAsync(string roleName);     
        Task<OperationResultModel> DeleteRoleByIdAsync(string roleId);
        Task<(OperationResultModel, IdentityRole role)> GetRoleNameByIdAsync(string roleId);
        Task<(OperationResultModel, string roleName, IEnumerable<ApplicationUser>)> GetUsersOfRoleAsync(string roleId);
    }
}
