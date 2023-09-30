using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Net;
using Microsoft.IdentityModel.Tokens;
using E_commerce.MVC_ASP.NET.Domain.Interfaces;
using E_commerce.MVC_ASP.NET.Models;

namespace E_commerce.MVC_ASP.NET.Controllers
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
        public IActionResult Email()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ResetPassword(string token, string userId)
        {
            ViewBag.Token = token;
            ViewBag.UserId = userId;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmarEmail()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Usuário não encontrado");
                return NotFound();
            }

            var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            ViewBag.IsEmailConfirmed = isEmailConfirmed;
            ViewBag.UserEmail = user.Email;
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

                    var currentDateClaim = new System.Security.Claims.Claim("CadastradoEm", DateTime.Now.ToString());
                    await _userManager.AddClaimAsync(user, currentDateClaim);

                    await _signInManager.SignInAsync(user, isPersistent: true);

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    string errorMessage;

                    switch (error.Code)
                    {
                        case "The Password field is required.":
                            errorMessage = "O campo de senha é obrigatório.";
                            break;
                        case "PasswordRequiresNonAlphanumeric":
                            errorMessage = "A senha deve conter pelo menos um caractere não alfanumérico.";
                            break;
                        case "PasswordRequiresLower":
                            errorMessage = "A senha deve conter pelo menos uma letra minúscula (a-z).";
                            break;
                        case "PasswordRequiresUpper":
                            errorMessage = "A senha deve conter pelo menos uma letra maiúscula (A-Z).";
                            break;
                        case "PasswordTooShort":
                            errorMessage = "A senha deve ter pelo menos 6 caracteres.";
                            break;
                        default:
                            errorMessage = "Ocorreu um erro ao criar a conta.";
                            break;
                    }

                    ModelState.AddModelError(string.Empty, errorMessage);
                }
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
        public async Task<IActionResult> Email(EmailModel emailModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(emailModel.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuário não encontrado.");
                    return View("Email");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var linkRedefinicao = Url.Action("ResetPassword", "Account", new { token, userId = user.Id }, Request.Scheme);

                var (success, errorMessage) = await _accountService.Email(user.Id, linkRedefinicao);

                if (success)
                {
                    ViewBag.ShowSuccessMessage = true;
                    return View();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, errorMessage);
                    return View("Email");
                }
            }
            return View("Email", "Account");
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Recupere o usuário com base no userId (ID do usuário).
                var user = await _userManager.FindByIdAsync(model.UserId);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuário não encontrado.");
                    return View(model);
                }

                // Redefina a senha usando o UserManager.
                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

                if (result.Succeeded)
                {
                    // A senha foi redefinida com sucesso. Você pode redirecionar para uma página de sucesso.
                    return RedirectToAction("ResetPasswordSuccess");
                }
                else
                {
                    // Ocorreu um erro ao redefinir a senha, exiba mensagens de erro.
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            // Se o modelo não for válido, você pode redirecionar de volta para o formulário com erros.
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEmail(bool? Email)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Usuário não encontrado ou email já verificado");
                return View();
            }

            if (Email == true)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("EmailVerificado", "Account", new { userId = user.Id, token }, Request.Scheme);
                var emailContent = $"Clique <a href=\"{confirmationLink}\">aqui</a> para confirmar seu Email.";

                var apiKey = "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";
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("lemosramonteste1997@gmail.com", "Confirmação de conta");
                var subject = "Confirmação de Email";
                var to = new EmailAddress(user.Email, user.UserName);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: emailContent);
                var result = await client.SendEmailAsync(msg);

                if (result.StatusCode == HttpStatusCode.Accepted)
                {
                    ViewBag.ShowEmailSuccessMessage = true;
                    return View();
                }

                ModelState.AddModelError(string.Empty, "Erro ao enviar o email de confirmação");
                return View();
            }

            ModelState.AddModelError(string.Empty, "Selecione a opção para confirmar o email");
            return View();
        }


        //------------------------------------------------------------------------------------------------------------------------------------------
    }
}
