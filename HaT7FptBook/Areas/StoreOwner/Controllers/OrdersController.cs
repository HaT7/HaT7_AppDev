using System.Linq;
using HaT7FptBook.Data;
using HaT7FptBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var oderDetails = _db.OderDetails.Include(a => a.Product).ToList();
            var oderHeader = _db.OrderHeaders
                .Include(a => a.ApplicationUser)
                .ToList();
            return View(oderHeader);
        }
    }
}