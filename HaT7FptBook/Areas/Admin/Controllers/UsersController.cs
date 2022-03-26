﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HaT7FptBook.Data;
using HaT7FptBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HaT7FptBook.Data;
using HaT7FptBook.Models;
using HaT7FptBook.Utility;
using Microsoft.AspNetCore.Mvc.Rendering;
using HaT7FptBook.ViewModels;

namespace HaT7FptBook.Areas.Admin.Controllers
{
    [Area(SD.Area_Admin)]
    [Authorize(Roles = SD.Role_Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        //============================== INDEX =====================================
        [HttpGet]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Index()
        {
            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var userList = _db.ApplicationUsers
                .Where(u => u.Id != claims.Value)
                .OrderBy(a => a.CreateAt)
                .ToList();

           foreach (var user in userList)
            {
                var userTemp = await _userManager.FindByIdAsync(user.Id);
                var roleTemp = await _userManager.GetRolesAsync(userTemp);
                user.Role = roleTemp.First();
            }

           return View(userList);
        }

        //================================= DELETE =================================
        [HttpGet]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(Index));
        }

        //=============================== UPDATE ====================================
        private IEnumerable<SelectListItem> GetRole()
        {
            return _roleManager.Roles.Select(a => a.Name).Select(a => new SelectListItem
            {
                Text = a,
                Value = a
            });
        }

        [HttpGet]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Update(string id)
        {
            if (id != null)
            {
                UpdateUserVM updataUserVm = new UpdateUserVM();
                var user = _db.ApplicationUsers.Find(id);
                updataUserVm.ApplicationUser = user;
                updataUserVm.RoleList = GetRole();
                var role = await _userManager.GetRolesAsync(user);
                updataUserVm.Role = role.FirstOrDefault();
                return View(updataUserVm);
            }

            return NotFound();
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Update(UpdateUserVM updataUserVm)
        {
            if (ModelState.IsValid)
            {
                var userInDb = _db.ApplicationUsers.Find(updataUserVm.ApplicationUser.Id);
                userInDb.FullName = updataUserVm.ApplicationUser.FullName;
                userInDb.Address = updataUserVm.ApplicationUser.Address;
                userInDb.PhoneNumber = updataUserVm.ApplicationUser.PhoneNumber;
                
                var oldRole = await _userManager.GetRolesAsync(userInDb);
                if (oldRole.First() != updataUserVm.Role)
                {
                    await _userManager.RemoveFromRoleAsync(userInDb, oldRole.First());
                    await _userManager.AddToRoleAsync(userInDb, updataUserVm.Role);
                }

                _db.ApplicationUsers.Update(userInDb);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            updataUserVm.RoleList = GetRole();
            return View(updataUserVm);
        }

        // ====================== LOCK & UNLOCK =======================
        [HttpGet]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> LockUnLock(string id)
        {
            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var user = _db.ApplicationUsers.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            if (user.Id == claims.Value)
            {
                return BadRequest();
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                // user is currently in lock, we will unlock
                user.LockoutEnd = DateTime.Now;
            }
            else
            {
                user.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        //======================= CONFIRM EMAIL ===========================
        [HttpGet]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> ConfirmEmail(string id)
        {
            var userInDb = _db.ApplicationUsers.Find(id);

            if (userInDb == null)
            {
                return NotFound();
            }

            ConfirmEmailVM confirmEmailVm = new ConfirmEmailVM()
            {
                Email = userInDb.Email
            };

            return View(confirmEmailVm);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailVM confirmEmailVm)
        {
            if (ModelState.IsValid)
            {
                var userInDb = await _userManager.FindByEmailAsync(confirmEmailVm.Email);
                if (userInDb != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(userInDb);
                    return RedirectToAction("ResetPassword", "Users",
                        new {token = token, email = userInDb.Email});
                }
            }

            return View(confirmEmailVm);
        }

        //======================= RESET PASSWORD ===========================
        [HttpGet]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }

            ResetPasswordVM resetPasswordVm = new ResetPasswordVM()
            {
                Email = email,
                Token = token
            };

            return View(resetPasswordVm);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM resetPasswordVm)
        {
            if (ModelState.IsValid)
            {
                var userInDb = await _userManager.FindByEmailAsync(resetPasswordVm.Email);
                if (userInDb != null)
                {
                    var result =
                        await _userManager.ResetPasswordAsync(userInDb, resetPasswordVm.Token,
                            resetPasswordVm.Password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            return View(resetPasswordVm);
        }
    }
}