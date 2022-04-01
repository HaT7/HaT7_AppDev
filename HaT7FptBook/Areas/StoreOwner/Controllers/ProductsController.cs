using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HaT7FptBook.Data;
using HaT7FptBook.Models;
using HaT7FptBook.Utility;
using HaT7FptBook.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HaT7FptBook.Areas.StoreOwner.Controllers
{
    [Area(SD.Area_StoreOwner)]
    [Authorize(Roles = SD.Role_StoreOwner)]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(ApplicationDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        //====================== INDEX ==========================   
        [HttpGet]
        [Authorize(Roles = SD.Role_StoreOwner)]
        public IActionResult Index()
        {
            var productList = _db.Products.Include(a => a.Category)
                .OrderBy(a => a.CreateAt).ToList();
            return View(productList);
        }

        //====================== DELETE ==========================
        [HttpGet]
        [Authorize(Roles = SD.Role_StoreOwner)]
        public IActionResult Delete(int? id)
        {
            var product = _db.Products.Find(id);
            _db.Products.Remove(product);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        //====================== UPSERT ==========================
        [NonAction]
        private IEnumerable<SelectListItem> CategorySelectListItems()
        {
            var categoryList = _db.Categories.ToList();
            var result = categoryList.Select(category => new SelectListItem
            {
                Text = category.Name,
                Value = category.Id.ToString()
            });

            return result;
        }

        [HttpGet]
        [Authorize(Roles = SD.Role_StoreOwner)]
        public IActionResult UpSert(int? id)
        {
            ProductUpSertVM productUpSertVM = new ProductUpSertVM();
            productUpSertVM.CategoryList = CategorySelectListItems();
            if (id == 0 || id == null)
            {
                productUpSertVM.Product = new Product();
                return View(productUpSertVM);
            }

            productUpSertVM.Product = _db.Products.Find(id);
            return View(productUpSertVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_StoreOwner)]
        public IActionResult UpSert(ProductUpSertVM productUpSertVm)
        {
            if (!ModelState.IsValid)
            {
                productUpSertVm.CategoryList = CategorySelectListItems();
                return View(productUpSertVm);
            }

            string webRootPath = _environment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(webRootPath, @"images/products");
                var extension = Path.GetExtension(files[0].FileName);
                var productDb = _db.Products.AsNoTracking().Where(a => a.Id == productUpSertVm.Product.Id).First();
                if (productDb.ImageUrl != null && productUpSertVm.Product.Id != 0)
                {
                    // to edit path so we need to delete the old path and update new one
                    var imagePath = Path.Combine(webRootPath, productDb.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    files[0].CopyTo(filesStreams);
                }

                productUpSertVm.Product.ImageUrl = @"/images/products/" + fileName + extension;
            }
            else
            {
                //update without change the images
                if (productUpSertVm.Product.Id != 0)
                {
                    Product objFromDb = _db.Products.Find(productUpSertVm.Product.Id);
                    productUpSertVm.Product.ImageUrl = objFromDb.ImageUrl;
                }
            }

            if (productUpSertVm.Product.Id == 0 || productUpSertVm.Product.Id == null)
            {
                _db.Products.Add(productUpSertVm.Product);
            }
            else
            {
                _db.Products.Update(productUpSertVm.Product);
            }

            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}