using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using HaT7FptBook.Data;
using HaT7FptBook.Models;
using HaT7FptBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HaT7FptBook.Areas.StoreOwner.Controllers
{
    [Area(SD.Area_StoreOwner)]
    [Authorize(Roles = SD.Role_StoreOwner)]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        public OrdersController(ApplicationDbContext db)
        {
            _db = db;
        }

        //======================== INDEX ==========================
        [HttpGet]
        public IActionResult Index()
        {
            List<int> listId = new List<int>();

            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var storeId = _db.Stores.FirstOrDefault(a => a.StoreOwnerId == claims.Value);

            if (storeId == null)
            {
                ViewData["Message"] = "Error: Store Id not exist. Let's create your Store";
                return RedirectToAction("Index", "Stores", new {area = "StoreOwner"});
            }
            
            var oderDetails = _db.OderDetails.Include(a => a.Book).ToList();
            foreach (var od in oderDetails)
            {
                var valid = od.Book.StoreId == storeId.Id;
                if (valid)
                {
                    listId.Add(od.OrderHeaderId);
                }
            }

            var listDistinId = new HashSet<int>(listId);

            var oderHeaderList = _db.OrderHeaders
                .Where(a => listDistinId.ToList().Contains(a.Id))
                .Include(a => a.ApplicationUser)
                .ToList();
            return View(oderHeaderList);
        }

        // ====================== ORDER DETAIL ===================================
        [HttpGet]
        public IActionResult OrderDetails(int orderHeaderId)
        {
            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var storeId = _db.Stores.FirstOrDefault(a => a.StoreOwnerId == claims.Value);
            
            var orderDetails = _db.OderDetails
                .Where(a => a.OrderHeaderId == orderHeaderId)
                .Include(a => a.Book)
                .ToList();

            var pIOD = new List<OderDetail>();
            foreach (var item in orderDetails)
            {
                var productInOrderDetails = item.Book.StoreId == storeId.Id;
                if (productInOrderDetails)
                {
                    pIOD.Add(item);
                }
            }
            
            return View(pIOD);
        }
    }
}