using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Areas.Admin.Models
{
    public class RoleEditModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public IEnumerable<IdentityUser>? Members { get; set; }
        public IEnumerable<IdentityUser>? NoMembers { get; set; }
        public string UserId { get; set; }
      
    }
}
