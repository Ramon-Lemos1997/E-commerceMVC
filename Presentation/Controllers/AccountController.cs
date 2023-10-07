﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Net;
using Contracts.Interfaces.Identity;
using Contracts.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Contracts.Interfaces.Infra.Data;
using Domain.Entities;

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

        [HttpGet]
        public IActionResult DataUser()
        {
            return View("DataUser");
        }



        //-------------------------------------------------------------------------------------------------------------------------------


        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var (user, result) = await _accountService.CreateUserAsync(model.Email, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(EmailModel emailModel)
        {
            if (ModelState.IsValid)
            {              
                var result = await _accountService.SendCode(emailModel.Email);

                if (result.Success)
                {
                    ViewBag.ShowSuccessMessage = true;
                    return View();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Message);
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
                    ViewBag.SuccessMessage = true;
                    return View();
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

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEmail(string Email)
        {
            var result = await _accountService.ConfirmEmailAsync(Email);
            if (result.Success)
            {
                ViewBag.SuccessMessage = true;
                return View();
            }
            else
            { 
                ModelState.AddModelError(string.Empty, result.Message);
                return View();
            }
        }
           
        public async Task<IActionResult> DataUser(DataUserModel model)
        {
            if (ModelState.IsValid)
            {     
                 return View();
            }
            
            return View(model);
        }




    //------------------------------------------------------------------------------------------------------------------------------------------
    }
}
