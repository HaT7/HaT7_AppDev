using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HaT7FptBook.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                await _userManager.DeleteAsync(user);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
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
        public async Task<IActionResult> Update(UpdateUserVM updateUserVm)
        {
            if (ModelState.IsValid)
            {
                var userInDb = await _db.ApplicationUsers.FindAsync(updateUserVm.ApplicationUser.Id);
                userInDb.FullName = updateUserVm.ApplicationUser.FullName;
                userInDb.Address = updateUserVm.ApplicationUser.Address;
                userInDb.PhoneNumber = updateUserVm.ApplicationUser.PhoneNumber;

                var oldRole = await _userManager.GetRolesAsync(userInDb);
                if (oldRole.First() != updateUserVm.Role)
                {
                    await _userManager.RemoveFromRoleAsync(userInDb, oldRole.First());
                    await _userManager.AddToRoleAsync(userInDb, updateUserVm.Role);
                }

                _db.ApplicationUsers.Update(userInDb);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            updateUserVm.RoleList = GetRole();
            return View(updateUserVm);
        }

        // ====================== LOCK & UNLOCK =======================
        [HttpGet]
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