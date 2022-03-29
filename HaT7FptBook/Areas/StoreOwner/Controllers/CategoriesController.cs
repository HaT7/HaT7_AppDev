using System.Linq;
using HaT7FptBook.Data;
using HaT7FptBook.Models;
using HaT7FptBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HaT7FptBook.Areas.StoreOwner.Controllers
{
    [Area(SD.Area_StoreOwner)]
    [Authorize(Roles = SD.Role_StoreOwner)]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        //======================== INDEX ==========================
        [HttpGet]
        [Authorize(Roles = SD.Role_StoreOwner)]
        public IActionResult Index()
        {
            var categoryList = _db.Categories.ToList();
            return View(categoryList);
        }

        //======================== DELETE ==========================
        [HttpGet]
        [Authorize(Roles = SD.Role_StoreOwner)]
        public IActionResult Delete(int id)
        {
            var category = _db.Categories.Find(id);
            _db.Categories.Remove(category);
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
                return View(new Category());
            }

            var category = _db.Categories.Find(id);
            return View(category);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_StoreOwner)]
        public IActionResult UpSert(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            if (category.Id == 0 || category.Id == null)
            {
                _db.Categories.Add(category);
            }
            else
            {
                _db.Categories.Update(category);
            }

            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}