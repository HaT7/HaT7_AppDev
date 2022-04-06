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

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            var products = _db.Products.Include(a => a.Category).ToList();
            return View(products);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var productFromDb = _db.Products.Include(a=>a.Category).FirstOrDefault(a => a.Id == id);
            Cart cart = new Cart()
            {
                Product = productFromDb,
                ProductId = productFromDb.Id
            };
            return View(cart);
        }
        
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize]
        public IActionResult Details(Cart CartObject)
        {
            CartObject.Id = 0;
            if (ModelState.IsValid)
            {
                //then we will add to cart
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.UserId = claim.Value;

                Cart cartFromDb = _db.Carts.FirstOrDefault(
                    u => u.UserId == CartObject.UserId && u.ProductId == CartObject.ProductId);

                if (cartFromDb == null)
                {
                    //no records exists in database for that product for that user
                    _db.Carts.Add(CartObject);
                }
                else
                {
                    cartFromDb.Count += CartObject.Count;
                    _db.Carts.Update(cartFromDb);
                }
                _db.SaveChanges();
                
                var count = _db.Carts
                    .Where(c => c.UserId == CartObject.UserId)
                    .ToList().Count();
                HttpContext.Session.SetInt32(SD.ssShoppingCart, count);
                
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var productFromDb = _db.Products.FirstOrDefault(u => u.Id == CartObject.ProductId);
                Cart shoppingCart = new Cart()
                {
                    Product = productFromDb,
                    ProductId = productFromDb.Id
                };
                return View(shoppingCart); 
            }
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