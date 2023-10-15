using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Contracts.Interfaces.Identity;
using Contracts.Models;
using Microsoft.AspNetCore.Authorization;
using Domain.Entities;

//Sigo um padrão basicamente em toda controller, onde espero receber um operationResultModel do servince, e quando necessário algum dado adicional, o operation
//possui Success = bit e Message = string

namespace Presentation.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountInterface _accountService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public AccountController(SignInManager<ApplicationUser> signInManager, IAccountInterface accountService)
        {
            _signInManager = signInManager;
            _accountService = accountService;
        }

        //________________________________________________________________________________________________________________________________________________________
        
        //verifica se o usuário já está conectado, se estiver joga para o index
        [HttpGet]
        public IActionResult Register() => (User.Identity.IsAuthenticated ? RedirectToAction("Index", "Home") : View());

        //verifica se o usuário já está conectado, se estiver joga para o index
        [HttpGet]
        public IActionResult Login() => (User.Identity.IsAuthenticated ? RedirectToAction("Index", "Home") : View());     

        [HttpGet]
        public async Task<IActionResult> AccessDenied() => View();
            
        [HttpGet]
        public IActionResult SendCode() => View();

        //verifica se o token já foi usado, tenho um column resetPassword, onde quando acontece o reset password mudo para true e faço esta verificação antes de exibir a tela 
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string userId)
        {          
            var result = await _accountService.CheckIfTokenResetPasswordIsUsedAsync(userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("Error");
            }

            ViewBag.Token = token;
            ViewBag.UserId = userId;

            return View();
        }

        //recupero o user do usuário atual conectado e também de o se o email do mesmo já foi confirmado para exibir corretamente a tela, permitindo ou não que envie um post para confirmar a conta
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail()
        {
            var (result, userEmail) = await _accountService.GetUserEmailAsync(User);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View();
            }

            var isEmailConfirmed = await _accountService.IsEmailConfirmedAsync(User);

            ViewBag.IsEmailConfirmed = isEmailConfirmed;
            ViewBag.UserEmail = userEmail;
            return View();
        }

        //recebo o id e o token e confirmo o seu email
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> VerifiedEmail(string userId, string token)
        {
            var result = await _accountService.VerifyEmailAsync(userId, token);

            if (result.Success)
            {
                ViewBag.ShowSuccessMessage = true;
                return View("VerifiedEmail");
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("Error");
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult MyAccount() => View();

        //recupero a informações do usuário atual para exibir
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> InfoUser()
        {
            var (result, userModel) = await _accountService.GetInfoUserAsync(User);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View();
            }

            return View(userModel);
        }

        //recupero a informações do usuário atual e atualizo
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UpdateInfoUser()
        {
            var (result, userModel) = await _accountService.GetInfoUserAsync(User);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View();
            }

            return View(userModel);
        }

        [Authorize]
        [HttpGet]
        public IActionResult UpdatePassword() => View();



        //-------------------------------------------------------------------------------------------------------------------------------

        
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var (result, user) = await _accountService.CreateUserAsync(model.Email, model.Password);

                if (result.Success)
                {
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    return RedirectToAction("Index", "Home");
                }
                
                ModelState.AddModelError(string.Empty, result.Message);
                return View();               
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Você excedeu o número de tentativas, aguarde 1 hora e tente novamente.");
                    return View();
                }

                ModelState.AddModelError(string.Empty, "Falha na autenticação. Verifique seu email/senha e tente novamente.");
            }
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        //enviar email para resetar a senha, pois esqueçeu da mesma
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(EmailModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountService.SendCodeAsync(model.Email);

                if (result.Success)
                {
                    ViewBag.ShowSuccessMessage = true;
                    return View();
                }
               
                ModelState.AddModelError(string.Empty, result.Message);
                return View();              
            }
            return View(model);
        }

        //resetar a senha 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountService.ResetPasswordAsync(model.UserId, model.Token, model.NewPassword);

                if (result.Success)
                {
                    ViewBag.SuccessMessage = true;
                    return View();
                }
              
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);              
            }
            return View(model);
        }

        //enviar um email para confirmar a conta através do email
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(string Email)
        {
            var result = await _accountService.ConfirmEmailAsync(Email);
            if (result.Success)
            {
                ViewBag.SuccessMessage = true;
                return View();
            }
            
            ModelState.AddModelError(string.Empty, result.Message);
            return View();           
        }

        //atualizar informações do usuário
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInfoUser(InfoUserModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountService.UpdateInfoUserAsync(model, User);
                if (result.Success)
                {
                    ViewBag.SuccessMessage = true;
                    return View();
                }
               
                ModelState.AddModelError(string.Empty, result.Message);
                return View();
            }
            return View(model);

        }

        //atualizar senha do usuário
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountService.UpdatePasswordAsync(model.NewPassword, model.CurrPassword, User);

                if (result.Success)
                {
                    ViewBag.SuccessMessage = true;
                    return View();
                }
               
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);                                            
            }

            return View(model);
        }




        //------------------------------------------------------------------------------------------------------------------------------------------
    }
}
