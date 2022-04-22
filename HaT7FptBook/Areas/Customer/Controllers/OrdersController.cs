using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using HaT7FptBook.Data;
using HaT7FptBook.Models;
using HaT7FptBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaT7FptBook.Areas.Customer.Controllers
{
    [Area(SD.Area_Customer)]
    [Authorize(Roles = SD.Role_Customer)]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        public OrdersController(ApplicationDbContext db)
        {
            _db = db;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var userId = _db.ApplicationUsers.FirstOrDefault(a => a.Id == claims.Value);

            try
            {
                var orderList = _db.OrderHeaders.Where(a => a.UserId== userId.Id).ToList();
                return View(orderList);
            }
            catch (Exception e)
            {
                Console.WriteLine("Order Error: " + e.Message);
                ViewData["Message"] = "Error: " + e.Message; 
            }

            return View(new List<OrderHeader>());
        }

        [HttpGet]
        public IActionResult OrderDetails(int orderHeaderId)
        {
            var orderDetails = _db.OderDetails
                .Where(a => a.OrderHeaderId == orderHeaderId)
                .Include(a => a.Product)
                .ToList();
            return View(orderDetails);
        }
    }
}