using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HaT7FptBook.Data;
using HaT7FptBook.Models;
using HaT7FptBook.Utility;
using HaT7FptBook.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;

namespace HaT7FptBook.Areas.StoreOwner.Controllers
{
    [Area(SD.Area_StoreOwner)]
    [Authorize(Roles = SD.Role_StoreOwner)]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _environment;
        private readonly int _recordsPerPage = 3;

        public BooksController(ApplicationDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        //====================== INDEX ==========================   
        [HttpGet]
        [Authorize(Roles = SD.Role_StoreOwner)]
        public async Task<IActionResult> Index(int id = 0, string searchString = "")
        {
            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var storeId = _db.Stores.FirstOrDefault(a => a.StoreOwnerId == claims.Value);

            if (storeId == null)
            {
                ViewData["Message"] = "Error: Store not exist. Let's create your Store and Category first";
                return RedirectToAction("Index", "Stores", new {area = "StoreOwner"});
            }
            
            if (storeId != null && !_db.Categories.Any(a => a.StoreId == storeId.Id))
            {
                ViewData["Message"] = "Error: Category not exist. Let's create your Category first";
                return RedirectToAction("Index", "Categories", new {area = "StoreOwner"});
            }

            try
            {
                var products = _db.Books
                    .Where(s => s.Title.Contains(searchString) || s.ISBN.Contains(searchString))
                    .Where(a => a.StoreId == storeId.Id)
                    .Include(a => a.Category)
                    .OrderBy(a => a.CreateAt)
                    .ToList();
                int numberOfRecords = products.Count();
                int numberOfPages = (int) Math.Ceiling((double) numberOfRecords / _recordsPerPage);
                ViewBag.numberOfPages = numberOfPages;
                ViewBag.currentPage = id;
                ViewData["CurrentFilter"] = searchString;
                var productList = products
                    .Skip(id * _recordsPerPage)
                    .Take(_recordsPerPage)
                    .Where(a => a.StoreId == storeId.Id)
                    .ToList();

                return View(productList);
            }
            catch (Exception e)
            {
                Console.WriteLine("Product Error: " + e.Message);
                ViewData["Message"] = "Error: " + e.Message;
            }

            return View(new List<Book>());
        }

        //====================== DELETE ==========================
        [HttpGet]
        [Authorize(Roles = SD.Role_StoreOwner)]
        public IActionResult Delete(int? id)
        {
            var product = _db.Books.Find(id);
            _db.Books.Remove(product);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        //====================== UPSERT ==========================
        [NonAction]
        private IEnumerable<SelectListItem> CategorySelectListItems()
        {
            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var storeId = _db.Stores.FirstOrDefault(a => a.StoreOwnerId == claims.Value);

            var categoryList = _db.Categories.Where(a => a.StoreId == storeId.Id).ToList();

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

            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var storeId = _db.Stores.FirstOrDefault(a => a.StoreOwnerId == claims.Value);

            if (id == 0 || id == null)
            {
                productUpSertVM.Book = new Book();
                productUpSertVM.Book.StoreId = storeId.Id;
                return View(productUpSertVM);
            }

            productUpSertVM.Book = _db.Books.Find(id);
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

            // IWebHostEnvironment Cung cấp thông tin về môi trường lưu trữ web mà ứng dụng đang chạy trong đó.
            // IWebHostEnvironment cũng sẽ kiểm tra được là mình đang ở chế độ development hay production

            // WebRootPath - Đường dẫn của thư mục www
            // WebRootPath − Path of the www folder(Gets or sets the absolute path to the directory that contains the
            // web-servable application content files)
            string webRootPath = _environment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
                string fileName = productUpSertVm.Book.ISBN + "_" + Guid.NewGuid();
                var uploads = Path.Combine(webRootPath, @"images/products");
                var extension = Path.GetExtension(files[0].FileName);
                if (productUpSertVm.Book.Id != 0)
                {
                    var productDb = _db.Books.AsNoTracking().Where(a => a.Id == productUpSertVm.Book.Id).First();
                    if (productDb.ImageUrl != null && productUpSertVm.Book.Id != 0)
                    {
                        // to edit path so we need to delete the old path and update new one
                        var imagePath = Path.Combine(webRootPath, productDb.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                }

                // Lớp FileStream tạo ra các objects để đọc và ghi dữ liệu ra file. Do stream là tài nguyên không
                // quản lý bởi GC, nên cần đưa nó vào cấu trúc using để tự động gọi giải phóng tài nguyên (Dispose)
                // khi hết khối lệnh.

                // Một luồng (stream) là một object được sử dụng để truyền dữ liệu. Khi dữ liệu truyền từ các nguồn
                // bên ngoài vào ứng dụng ta gọi đó là đọc stream, và khi dữ liệu truyền từ chương trình ra nguồn bên
                // ngoài ta gọi nó là ghi stream.

                // FileMode.Create để tạo mới một file để lưu, nếu file đó đã tồn tại thì nó sẽ ghi đè lên
                using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    files[0].CopyTo(filesStreams);
                }

                productUpSertVm.Book.ImageUrl = @"/images/products/" + fileName + extension;
            }
            else
            {
                //update without change the images
                if (productUpSertVm.Book.Id != 0)
                {
                    Book objFromDb = _db.Books
                        .AsNoTracking()
                        .Where(a => a.Id == productUpSertVm.Book.Id)
                        .First();
                    productUpSertVm.Book.ImageUrl = objFromDb.ImageUrl;
                }
            }

            if (_db.Books.Any(a => a.ISBN.ToLower().Trim() == productUpSertVm.Book.ISBN.ToLower().Trim() && a.Id != productUpSertVm.Book.Id))
            {
                ViewData["Message"] = "Error: ISBN already exist";
                productUpSertVm.CategoryList = CategorySelectListItems();
                return View(productUpSertVm);
            }
            
            if (productUpSertVm.Book.Id == 0 || productUpSertVm.Book.Id == null)
            {
                _db.Books.Add(productUpSertVm.Book);
            }
            else
            {
                _db.Books.Update(productUpSertVm.Book);
            }

            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // ================= DETAIL ===================
        [HttpGet]
        public IActionResult Details(int id)
        {
            var productFromDb = _db.Books
                .Include(a => a.Category)
                .FirstOrDefault(a => a.Id == id);
            return View(productFromDb);
        }
    }
}