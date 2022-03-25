using System;
using System.Linq;
using HaT7FptBook.Data;
using HaT7FptBook.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HaT7FptBook.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext db, UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initializer()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception e)
            {
                // ignored
            }
            
            if (_db.Roles.Any(r => r.Name == "Admin")) return;
            if (_db.Roles.Any(r => r.Name == "StoreOwner")) return;
            if (_db.Roles.Any(r => r.Name == "Customer")) return;
            
            _roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole("StoreOwner")).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole("Customer")).GetAwaiter().GetResult();

            _userManager.CreateAsync(new ApplicationUser()
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                EmailConfirmed = true, 
                FullName = "Admin",
                Address = "Đà Nẵng"
            }, "Admin123@").GetAwaiter().GetResult();

            ApplicationUser admin = _db.ApplicationUsers.Where(u => u.Email == "admin@gmail.com").FirstOrDefault();
            _userManager.AddToRoleAsync(admin, "Admin").GetAwaiter().GetResult();
        }
    }
}