using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HaT7FptBook.Data;
using HaT7FptBook.Models;
using HaT7FptBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HaT7FptBook.Areas.StoreOwner.Controllers
{
    [Area(SD.Area_StoreOwner)]
    [Authorize(Roles = SD.Role_StoreOwner)]
    public class StoresController : Controller
    {
        private readonly ApplicationDbContext _db;

        public StoresController(ApplicationDbContext db)
        {
            _db = db;
        }

        //======================== INDEX ==========================
        [HttpGet]
        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            try
            {
                var store = _db.Stores.Where(a => a.StoreOwnerId == claims.Value).ToList();
                return View(store);
            }
            catch (Exception e)
            {
                Console.WriteLine("Store Error: " + e.Message);
                ViewData["Message"] = "Error: " + e.Message;
            }

            return View(new List<Store>());
        }

        //======================== DELETE ==========================
        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                var store = await _db.Stores.FindAsync(id);
                if (store == null)
                {
                    return Json(new { success = false, message = "Error while Deleting" });
                }
                _db.Stores.Remove(store);
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Delete Successful" });
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
                return Json(new { success = false, message = e.Message });
            }
        }

        //======================== UPSERT ==========================
        [HttpGet]
        public IActionResult UpSert(int? id)
        {
            if (id == 0 || id == null)
            {
                return View(new Store());
            }

            var store = _db.Stores.Find(id);
            return View(store);
        }

        [HttpPost]
        public IActionResult UpSert(Store store)
        {
            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            store.StoreOwnerId = claims.Value;

            if (_db.Stores.Any(a =>
                    a.Name.ToLower().Trim() == store.Name.ToLower().Trim() && a.StoreOwnerId != store.StoreOwnerId))
            {
                ViewData["Message"] = "Error: Store name already exist";
                return View(store);
            }

            if (store.Id == 0 || store.Id == null)
            {
                var checkStoreExist = _db.Stores.Any(a => a.StoreOwnerId == claims.Value);
                if (checkStoreExist)
                {
                    return BadRequest();
                }

                _db.Stores.Add(store);
            }
            else
            {
                _db.Stores.Update(store);
            }

            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        //======================== HELP ==========================
        public IActionResult Help()
        {
            return View();
        }
    }
}