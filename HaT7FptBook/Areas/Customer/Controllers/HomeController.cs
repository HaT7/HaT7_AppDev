using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HaT7FptBook.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HaT7FptBook.Models;
using HaT7FptBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HaT7FptBook.Areas.Customer.Controllers
{
    [Area(SD.Area_Customer)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly int _recordsPerPage = 20;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index(int id = 0, string searchString = "")
        {
            if (User.IsInRole(SD.Role_StoreOwner))
            {
                return RedirectToAction("Index", "Stores", new {area = "StoreOwner"});
            }
            
            var products = _db.Books
                .Where(s => s.Title.Contains(searchString) || s.Category.Name.Contains(searchString))
                .Include(a => a.Category)
                .ToList();
            int numberOfRecords = products.Count();
            int numberOfPages = (int) Math.Ceiling((double) numberOfRecords / _recordsPerPage);
            
            ViewBag.numberOfPages = numberOfPages;
            ViewBag.currentPage = id;
            ViewData["CurrentFilter"] = searchString;
            var productList = products.Skip(id * _recordsPerPage).Take(_recordsPerPage).ToList();

            return View(productList);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            // Thực chất là hiện ra một cái cart dưới dạng product detail (có hiện ra cái count của cart nữa)
            var productFromDb = _db.Books
                .Include(a => a.Category)
                .FirstOrDefault(a => a.Id == id);
            Cart cart = new Cart()
            {
                Book = productFromDb,
                BookId = productFromDb.Id
            };
            return View(cart);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize]
        public IActionResult Details(Cart CartObject)
        {
            // Ở trên phần HttpGet chỉ hiện ra một cái cart dưới dạng product detail và chỉ có thêm một object là Count
            // mà trong cart chúng ta còn rất nhiều object nữa nên ở đây chúng ta xét CartObject.Id = 0 để nó mặc định
            // với trường hợp là tạo mới một cái cart.
            CartObject.Id = 0;
            if (ModelState.IsValid)
            {
                // Đầu tiên chúng ta lấy Id mà user đang đăng nhập
                var claimsIdentity = (ClaimsIdentity) User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.UserId = claim.Value;

                Cart cartFromDb = _db.Carts
                    .FirstOrDefault(u => u.UserId == CartObject.UserId && u.BookId == CartObject.BookId);

                if (cartFromDb == null)
                {
                    _db.Carts.Add(CartObject);
                }
                else
                {
                    cartFromDb.Count += CartObject.Count;
                    _db.Carts.Update(cartFromDb);
                }

                _db.SaveChanges();

                // Ở đây dữ liệu count sẽ được lưu trong một cái session, cụ thể là ssShoppingCart (vào file SD xem cụ
                // thể là Shopping Cart Session)
                var count = _db.Carts
                    .Where(c => c.UserId == CartObject.UserId)
                    .ToList().Count();
                HttpContext.Session.SetInt32(SD.ssShoppingCart, count);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var productFromDb = _db.Books
                    .Include(a => a.Category)
                    .FirstOrDefault(u => u.Id == CartObject.BookId);
                Cart cart = new Cart()
                {
                    Book = productFromDb,
                    BookId = productFromDb.Id
                };
                return View(cart);
            }
        }

        // ============ HELP ================
        public IActionResult Help()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}