using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ExcelDataReader;
using HaT7FptBook.Data;
using HaT7FptBook.Models;
using HaT7FptBook.Utility;
using HaT7FptBook.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HaT7FptBook.Areas.StoreOwner.Controllers
{
    [Area(SD.Area_StoreOwner)]
    [Authorize(Roles = SD.Role_StoreOwner)]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _environment;
        public CategoriesController(ApplicationDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        //======================== INDEX ==========================
        [HttpGet]
        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var storeId = _db.Stores.FirstOrDefault(a => a.StoreOwnerId == claims.Value);

            if (storeId == null)
            {
                ViewData["Message"] = "Error: Store Id not exist. Let's create your Store first";
                return RedirectToAction("Index", "Stores", new { area = "StoreOwner"});
            }
            
            try
            {
                var categoryList = _db.Categories.Where(a => a.StoreId == storeId.Id).ToList();
                return View(categoryList);
            }
            catch (Exception e)
            {
                Console.WriteLine("Category Error: " + e.Message);
                ViewData["Message"] = "Error: " + e.Message; 
            }
            return View(new List<Category>());
        }

         //======================== DELETE ==========================
         [HttpDelete]
         public async Task<IActionResult> Delete(int id)
         {

             try
             {
                 var category = await _db.Categories.FindAsync(id);
                 if (category == null)
                 {
                     return Json(new { success = false, message = "Error while Deleting" });
                 }
                 _db.Categories.Remove(category);
                 _db.SaveChanges();
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
             var claimIdentity = (ClaimsIdentity) User.Identity;
             var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

             var storeId = _db.Stores.FirstOrDefault(a => a.StoreOwnerId == claims.Value);

             if (storeId == null)
             {
                 ViewData["Message"] = "Error: Store Id not exist. Let's create your Store first";
                 return RedirectToAction("Index", "Stores", new { area = "StoreOwner"});
             }

             if (id == 0 || id == null)
             {
                 var categoryCreate = new Category();
                 categoryCreate.StoreId = storeId.Id;
                 return View(categoryCreate);
             }
             var category = _db.Categories.Find(id);
             return View(category);
         }
         [HttpPost]
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
         
         //==================== UPLOAD EXCEL ========================
         [HttpPost]
         public IActionResult UploadExcel(IFormFile file)
         {
             var claimIdentity = (ClaimsIdentity) User.Identity;
             var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

             var storeId = _db.Stores.FirstOrDefault(a => a.StoreOwnerId == claims.Value);
             
             if (file != null)
             {
                 //create folder
                 var path = Path.Combine(_environment.WebRootPath, "Uploads");
                 // check if folder is existed if not then create new one
                 if (!Directory.Exists(path))
                 {
                     Directory.CreateDirectory(path);
                 }

                 string fileName = Path.GetFileName(file.FileName);;
                 string filePath = Path.Combine(path, fileName);
                 // store file
                 using (FileStream stream = new FileStream(filePath, FileMode.Create))
                 {
                     file.CopyTo(stream);
                 }

                 using var streamFile = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read);
                 using var reader = ExcelReaderFactory.CreateReader(streamFile);
                 while (reader.Read())
                 {
                     var category = new Category()
                     {
                         StoreId = storeId.Id,
                         Name = reader.GetValue(0).ToString(),
                         Description = reader.GetValue(1).ToString()
                     };

                     _db.Categories.Add(category);
                 }

                 _db.SaveChanges();
                 return RedirectToAction("Index");
             }

             ViewData["Message"] = "Please choose file"; 
             return RedirectToAction(nameof(UpSert));
         }
    }
}