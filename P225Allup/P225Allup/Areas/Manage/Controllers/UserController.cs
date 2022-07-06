﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P225Allup.Areas.Manage.ViewModels.AccountViewModels;
using P225Allup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P225Allup.Areas.Manage.Controllers
{
    [Area("manage")]
    [Authorize(Roles ="SuperAdmin")]
    public class UserController : Controller
    {

        private readonly UserManager<AppUser> _userManager;

        public UserController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(bool? status)
        {
            List<AppUser> query = await _userManager.Users.Where(u=>u.UserName != User.Identity.Name).ToListAsync();

            if (status != null)
            {
                query = query.Where(b => b.IsDeActive == status).ToList();
            }

            ViewBag.Status = status;

            foreach (AppUser item in query)
            {
                var roles = await _userManager.GetRolesAsync(item);
                item.Role = roles[0];
            }

            return View(query);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            AppUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string id,ResetPasswordVM resetPasswordVM)
        {
            if (!ModelState.IsValid) return View();

            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            AppUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            //string emailconfirmToke = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

            await _userManager.ResetPasswordAsync(appUser, token, resetPasswordVM.Password);
            //await _userManager.ConfirmEmailAsync(appUser, emailconfirmToke);

            return RedirectToAction("index");
        }
        [HttpGet]
        public IActionResult Create()
        {
            return  View();
        }

        [HttpPost]

        public async Task<IActionResult> Create(UserCreateVM userCreateVM)
        {

            if (!ModelState.IsValid)
            {
                return View();
            };

            AppUser appUser = new AppUser
            {
                Name = userCreateVM.Name,
                SurName = userCreateVM.SurName,
                FatherName = userCreateVM.FatherName,
                Age = userCreateVM.Age,
                Email = userCreateVM.Email,
                UserName = userCreateVM.UserName
            };  

            IdentityResult identityResult = await _userManager.CreateAsync(appUser, userCreateVM.Password);

            if (!identityResult.Succeeded)
            {
                foreach (var item in identityResult.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }

                return View();
            }
            if (userCreateVM.IsAdmin==true)
            {
                await _userManager.AddToRoleAsync(appUser, "Admin");
            }
            else
            {
                await _userManager.AddToRoleAsync(appUser, "Member");
            }
            

            return RedirectToAction("index","user",new { area="manage"});
        }
    }
}
