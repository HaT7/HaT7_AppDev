using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HaT7FptBook.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HaT7FptBook.Models;
using HaT7FptBook.Utility;
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