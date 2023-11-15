using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Domain.Interfaces.Identity;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Domain.Entities;
using Stripe;
using Stripe.Checkout;

//Sigo um padrão basicamente em toda controller, onde espero receber um operationResultModel do servince, e quando necessário algum dado adicional, o operation
//possui Success = bit e Message = string

namespace Presentation.Controllers
{
    public class AccountController : Controller
    {
        const string secret = "whsec_907dc1d3de674873c6704c793981386a16e13471226fa19a951a84352d8e60208";
        private readonly IAccountInterface _accountService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public AccountController(SignInManager<ApplicationUser> signInManager, IAccountInterface accountService)
        {
            _signInManager = signInManager;
            _accountService = accountService;
        }

        //________________________________________________________________________________________________________________________________________________________
        
        /// <summary>
        /// verifica se o usuário já está conectado, se estiver joga para o index
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register() => (User.Identity.IsAuthenticated ? RedirectToAction("Index", "Home") : View());

        /// <summary>
        /// verifica se o usuário já está conectado, se estiver joga para o index
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Login() => (User.Identity.IsAuthenticated ? RedirectToAction("Index", "Home") : View());     

        [HttpGet]
        public async Task<IActionResult> AccessDenied() => View();
            
        [HttpGet]
        public IActionResult SendCode() => View();

        /// <summary>
        /// verifica se o token já foi usado, tenho um column resetPassword, onde quando acontece o reset password mudo para true e faço esta verificação antes de exibir a tela 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// recupero o user do usuário atual conectado e também de o se o email do mesmo já foi confirmado para exibir corretamente a tela, permitindo ou não que envie um post para confirmar a conta
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// recebo o id e o token e confirmo o seu email
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
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

            ModelState.AddModelError(string.Empty, result.Message);
            return View("Error");           
        }

        [Authorize]
        [HttpGet]
        public IActionResult MyAccount() => View();

        ///recupero a informações do usuário atual para exibir
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

        /// <summary>
        /// recupero a informações do usuário atual e atualizo
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// enviar email para resetar a senha, pois esqueçeu da mesma
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// resetar a senha 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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
            ViewBag.UserId = model.UserId;
            ViewBag.Token = model.Token;
            return View(model);
        }

        /// <summary>
        /// enviar um email para confirmar a conta através do email
        /// </summary>
        /// <param name="Email"></param>
        /// <returns></returns>
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

        /// <summary>
        /// atualizar informações do usuário
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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
                    TempData["MessageSuccess"] = "Seus dados foram atualizados com sucesso.";
                    return RedirectToAction(nameof(InfoUser));
                }
               
                ModelState.AddModelError(string.Empty, result.Message);
                return View();
            }
            
            return View(model);
        }

        /// <summary>
        /// atualizar senha do usuário
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountService.UpdatePasswordAsync(model.NewPassword, model.CurrPassword, User);

                if (result.Success)
                {
                    TempData["MessageSuccess"] = "Senha atualizada com sucesso.";
                    return RedirectToAction(nameof(InfoUser));
                }
               
                ModelState.AddModelError(string.Empty, result.Message);
                return View();                                            
            }

            return View(model);
        }


      



        //------------------------------------------------------------------------------------------------------------------------------------------
    }
}
