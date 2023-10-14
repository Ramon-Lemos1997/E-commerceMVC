using Contracts.Models;
using Microsoft.AspNetCore.Identity;
using Presentation.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Interfaces.Identity
{
   public interface IAdminRoleInterface
    {
        Task<OperationResultModel> CreateRoleAsync(string roleName);     
        Task<OperationResultModel> DeleteRoleByIdAsync(string roleId);
        Task<(OperationResultModel, IdentityRole role)> GetRoleNameByIdAsync(string roleId);
    }
}
