using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Net;
using Contracts.Interfaces.Identity;
using Contracts.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Contracts.Interfaces.Infra.Data;

namespace Presentation.Controllers
{
    
    public class AccountController : Controller
    {
        private readonly IAccountInterface _accountService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration, ISendEmail sendEmail, IAccountInterface accountService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AccessDenied()
        {
            return View();
        }
        
        [HttpGet]
        public IActionResult SendCode()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string userId)
        {
            var (success, errorMessage) = await _accountService.CheckIfTokenResetPasswordIsUsedAsync(userId);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return View("Error");
            }

            ViewBag.Token = token;
            ViewBag.UserId = userId;

            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ConfirmarEmail()
        {
            var (success, errorMessage, userEmail) = await _accountService.GetUserEmailAsync(User);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return View();
            }

            var isEmailConfirmed = await _accountService.IsEmailConfirmedAsync(User);

            ViewBag.IsEmailConfirmed = isEmailConfirmed;
            ViewBag.UserEmail = userEmail;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EmailVerificado(string userId, string token)
        {
           
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError(string.Empty, "Erro ao validar sua chave, tente novamente.");
                return View("Error");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Este link já foi usado.");
                return View("Error");
            }

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Usuário não encontrado.");
                return View("Error");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                ViewBag.ShowSuccessMessage = true;
                return View("EmailVerificado");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Erro ao confirmar o email, tente novamente ou entre em contato com o administrador.");
                return View("Error");
            }
        }



        //-------------------------------------------------------------------------------------------------------------------------------


        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");

                    var currentDateClaim = new Claim("CadastradoEm", DateTime.Now.ToString());
                    await _userManager.AddClaimAsync(user, currentDateClaim);

                    await _signInManager.SignInAsync(user, isPersistent: true);

                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
             
                                 
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Login Inválido");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync(); // Faz o logout do usuário

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(EmailModel emailModel)
        {
            if (ModelState.IsValid)
            {              
                var (success, errorMessage) = await _accountService.SendCode(emailModel.Email);

                if (success)
                {
                    ViewBag.ShowSuccessMessage = true;
                    return View();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, errorMessage);
                    return View();
                }
            }
            return View("Email", "Account");
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountService.ResetPasswordAsync(model.UserId, model.Token, model.NewPassword);

                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordSuccess");
                }
                else
                {                  
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEmail(string Email)
        {
            var (success, errorMessage) = await _accountService.ConfirmEmailAsync(Email);
            if (success)
            {
                ViewBag.SuccessMessage = true;
                return View();
            }
            else
            { 
                ModelState.AddModelError(string.Empty, errorMessage);
                return View();
            }
        }
           
    




    //------------------------------------------------------------------------------------------------------------------------------------------
    }
}
