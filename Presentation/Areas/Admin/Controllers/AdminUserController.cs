using Contracts.Interfaces.Identity;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ("Admin"))]
    public class AdminUserController : Controller
    {
        private readonly IAdminUserInterface _adminUserService;
        private readonly IAccountInterface _accountService;

        public AdminUserController(UserManager<ApplicationUser> userManager, IAdminUserInterface adminUserService, IAccountInterface accountService)
        {
            _adminUserService = adminUserService;
            _accountService = accountService;
        }

        //------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (result, users) = await _adminUserService.GetUsersAsync();

            if (result.Success)
            {
                return View(users);
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View();    
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var (result, userModel) = await _adminUserService.GetInfoUserByIdAsync(id);

            if (result.Success)
            {
                return View(userModel);
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View();
        }

        //recupero a informações do usuário selecionado
        [HttpGet]
        public async Task<IActionResult> InfoUser(string id)
        {
            var (result, userModel) = await _adminUserService.GetInfoUserByIdAsync(id);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View();
            }

            return View(userModel);
        }

        //recupero a informações do usuário selecionado
        [HttpGet]
        public async Task<IActionResult> UpdateRole(string id)
        {
            var (result, userModel) = await _adminUserService.GetInfoUserByIdAsync(id);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View();
            }

            return View(userModel);
        }

        //-----------------------------------------------------------------------------------------------------------------------


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(string userId, string selectedRole)
        {


            var result = await _adminUserService.UpdateRoleUserAsync(User, userId, selectedRole);

            if (result.Success)
            {
                TempData["MessageSuccess"] ="O privilégio do usuário foi redefinido com sucesso.";
                return RedirectToAction("Index");
            }

            TempData["MessageError"] = result.Message;
            return RedirectToAction(nameof(Index));
            
        }



        [HttpPost, ActionName("Delete")] //o actionName permite que tenha métodos iguais
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _adminUserService.DeleteUserAsync(userId);

            if(result.Success)
            {
                TempData["MessageSuccess"] = "Usuário excluído com sucesso.";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View();
        }


    }
}
