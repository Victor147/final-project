﻿using AutoMapper;
using final_project.Data.Entities;
using final_project.Models;
using final_project.Services.OrderService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace final_project.Controllers;

public class ProfileController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IOrderService _orderService;
    private readonly IMapper _mapper;

    public ProfileController(UserManager<User> userManager, IOrderService orderService, IMapper mapper)
    {
        _userManager = userManager;
        _orderService = orderService;
        _mapper = mapper;
    }

    [Authorize(Roles = "User")]
    public async Task<IActionResult> ActiveUserOrders()
    {
        var username = HttpContext.User.Identity!.Name;
        var user = await _userManager.FindByNameAsync(username);

        var orders = await _orderService.GetAllUnfinishedOrdersForUserAsync(user.Id);
        
        return View(orders);
    }
    
    [Authorize(Roles = "User")]
    public async Task<IActionResult> FinishedUserOrders()
    {
        var username = HttpContext.User.Identity!.Name;
        var user = await _userManager.FindByNameAsync(username);

        var orders = await _orderService.GetAllFinishedOrdersForUserAsync(user.Id);
        
        return View(orders);
    }

    [Authorize(Roles = "User")]
    public async Task<IActionResult> Profile()
    {
        var username = HttpContext.User.Identity!.Name;
        var user = await _userManager.FindByNameAsync(username);

        var model = _mapper.Map<ProfileModel>(user);
        
        return View(model);
    }
}