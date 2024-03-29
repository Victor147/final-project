﻿using AutoMapper;
using final_project.Data.Entities;
using final_project.Helpers;
using final_project.Models;
using final_project.Services.EmailService;
using final_project.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace final_project.Controllers;

public class AuthenticationController : Controller
{
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IEmailSender _emailService;

    public AuthenticationController(IMapper mapper, UserManager<User> userManager, RoleManager<Role> roleManager,
        SignInManager<User> signInManager, IEmailSender emailService)
    {
        _mapper = mapper;
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _emailService = emailService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? beforePath)
    {
        if (beforePath == null)
        {
            return View(new RegisterModel
            {
                BeforePath = string.Empty
            });
        }

        TempData["RegisterMessage"] = "Изисква се регистрация или вписване, за да можете да добавите продукт!";

        return View(new RegisterModel
        {
            BeforePath = beforePath
        });
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterModel viewModel)
    {
        if (ModelState.IsValid)
        {
            if (viewModel.Password != viewModel.RepeatPassword)
            {
                ModelState.AddModelError("RepeatPassword", "Паролите не съвпадат!");
                return View("Register", viewModel);
            }

            var user = _mapper.Map<User>(viewModel);
            var result = await _userManager.CreateAsync(user, viewModel.Password);

            if (result.Succeeded)
            {
                var userRole = await _roleManager.FindByNameAsync("User");
                await _userManager.AddToRoleAsync(user, userRole.Name);
                await _signInManager.SignInAsync(user, true);

                if (viewModel.BeforePath != null)
                {
                    var paths = viewModel.BeforePath.Split('/');
                    var action = paths[0];
                    var controller = paths[1];
                    return viewModel.BeforePath != null
                        ? RedirectToAction(action, controller)
                        : RedirectToAction("Index", "Home");
                }

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ModelState.AddModelError(string.Empty, "Невалиден опит за регистрация!");
        }


        return View("Register", viewModel);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? beforePath)
    {
        if (beforePath == null)
        {
            return View(new LoginModel
            {
                BeforePath = string.Empty
            });
        }

        return View(new LoginModel
        {
            BeforePath = beforePath
        });
    }


    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(viewModel.Username, viewModel.Password,
                viewModel.RememberMe, false);
            if (result.Succeeded)
            {
                if (viewModel.BeforePath != null)
                {
                    var paths = viewModel.BeforePath.Split('/');
                    var action = paths[0];
                    var controller = paths[1];
                    return viewModel.BeforePath != null
                        ? RedirectToAction(action, controller)
                        : RedirectToAction("Index", "Home");
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("Password", "Грешна парола, опитайте отново!");
        }

        return View("Login", viewModel);
    }

    [HttpGet]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> UpdateProfile(string username)
    {
        var user = await _userManager.FindByNameAsync(username);

        var profile = _mapper.Map<ProfileModel>(user);

        return View(profile);
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileModel model)
    {
        if (ModelState.IsValid)
        {
            var fromDb = await _userManager.FindByNameAsync(model.Username);
            fromDb.FirstName = model.FirstName;
            fromDb.LastName = model.LastName;
            fromDb.Address = model.Address;
            fromDb.Email = model.Email;

            await _userManager.UpdateAsync(fromDb);

            return RedirectToAction("Profile", "Profile");
        }

        return View("UpdateProfile", model);
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return View("ForgotPassword");
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Authentication", new { userId = user.Id, token = token },
                protocol: Request.Scheme);

            MessageHelper helper = new MessageHelper
            {
                Name = user.FirstName + " " + user.LastName,
                To = user.Email,
                Subject = "Промяна на паролата",
                Content = callbackUrl
            };

            _emailService.SendEmail(helper);

            return RedirectToAction("ForgotPasswordSuccess", "Authentication");
        }

        return View("ForgotPassword", model);
    }

    [HttpGet]
    public IActionResult ForgotPasswordSuccess()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(int userId, string token)
    {
        ResetPasswordViewModel model = new ResetPasswordViewModel
        {
            Token = token
        };

        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Паролите не съвпадат!");
                return View("ResetPassword", model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
            {
                return RedirectToAction("ResetPassword", model);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordSuccess");
            }
        }

        return View("ResetPassword", model);
    }

    [HttpGet]
    public IActionResult ResetPasswordSuccess()
    {
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}