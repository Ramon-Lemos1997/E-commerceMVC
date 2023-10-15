using Contracts.Interfaces.Identity;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminRoleController : Controller
    {      
        private readonly IAdminRoleInterface _adminRoleService;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AdminRoleController(RoleManager<IdentityRole> roleManager, IAdminRoleInterface adminRoleService, UserManager<ApplicationUser> userManager)
        {
            _adminRoleService = adminRoleService;
            _roleManager = roleManager;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------


        [HttpGet]
        public ViewResult Index() => (_roleManager.Roles != null ? View(_roleManager.Roles) : View());
        
        [HttpGet]
        public ViewResult Create() => View();

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var roleId = id;
            var (result, role) = await _adminRoleService.GetRoleNameByIdAsync(roleId);

            if (result.Success)
            {
                return View(role);
            }
        
            ModelState.AddModelError(string.Empty, result.Message);
            return View();          
        }   


        //------------------------------------------------------------------------------------------------------------------------



        [HttpPost]
        public async Task<IActionResult> Create(string roleName)
        {
            if (ModelState.IsValid)
            {
                var result = await _adminRoleService.CreateRoleAsync(roleName);
                if (result.Success)
                {
                    TempData["MessageSuccess"] = "Regra criada com sucesso.";
                    return RedirectToAction("Index");
                }
           
                
                ModelState.AddModelError(string.Empty, result.Message);
                return View();
                
            }

            return View();
        }

        [HttpPost, ActionName("Delete")] //o actionName permite que tenha métodos iguais
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> DeleteConfirmed(string id)
        {       
            var result = await _adminRoleService.DeleteRoleByIdAsync(id);

            if (result.Success)
            {
                TempData["MessageSuccess"] = "Regra excluída com sucesso.";
                return RedirectToAction("Index");
            }
           
            ModelState.AddModelError(string.Empty, result.Message);
            return View();
            
        }














    }
}
