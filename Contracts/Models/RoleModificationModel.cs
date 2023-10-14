using System.ComponentModel.DataAnnotations;

namespace Presentation.Areas.Admin.Models
{
    public class RoleModificationModel
    {
        [Required]
        public string RoleName { get; set; }
        public string? RoleId { get; set; }
        public string[]? AddIds { get; set; }
        public string[]? DeleteIds  { get; set; }   
    }
}
