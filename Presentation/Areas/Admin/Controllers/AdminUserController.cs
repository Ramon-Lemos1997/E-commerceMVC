using Application.Services.Account;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminUserController(UserManager<ApplicationUser> userManager)
        {
           this._userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var users = _userManager.Users;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            // Verifique se o usuário existe
            if (user == null)
            {
                return NotFound();
            }
            ViewBag.Success = true;
            return View(user);
        }



        [HttpPost, ActionName("Delete")] //o actionName permite que tenha métodos iguais
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if(user == null)
            {
                ViewBag.errormessage = $"usuário com id = {id} não foi encontrado";
                return View("NotFound");
            }
            else
            {
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("Index");
            }
        }


    }
}
