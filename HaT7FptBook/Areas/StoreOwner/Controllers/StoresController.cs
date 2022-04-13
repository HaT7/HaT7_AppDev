using System.Linq;
using System.Security.Claims;
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
        [Authorize(Roles = SD.Role_StoreOwner)]
        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            
            var storeList = _db.Stores.Where(a=>a.StoreOwnerId == claims.Value).ToList();
            return View(storeList);
        }

        //======================== DELETE ==========================
        [HttpGet]
        [Authorize(Roles = SD.Role_StoreOwner)]
        public IActionResult Delete(int id)
        {
            var store = _db.Stores.Find(id);
            _db.Stores.Remove(store);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        
        //======================== UPSERT ==========================
        [HttpGet]
        [Authorize(Roles = SD.Role_StoreOwner)]
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
        [Authorize(Roles = SD.Role_StoreOwner)]
        public IActionResult UpSert([Bind("Id,Name")]Store store)
        {
            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            store.StoreOwnerId = claims.Value;

            if (store.Id == 0 || store.Id == null)
            {
                var checkStoreExist = _db.Stores.Any(a=> a.StoreOwnerId == claims.Value);
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
    }
}