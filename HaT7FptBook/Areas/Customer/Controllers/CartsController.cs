using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using HaT7FptBook.Data;
using HaT7FptBook.Models;
using HaT7FptBook.ViewModels;
using HaT7FptBook.Utility;
using HaT7FptBook.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HaT7FptBook.Areas.Customer.Controllers
{
    [Area(SD.Area_Customer)]
    [Authorize(Roles = SD.Role_Customer)]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CartsController(ApplicationDbContext db)
        {
            _db = db;
        }
        [BindProperty] public ShoppingCartVM ShoppingCartVM { get; set; }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new Models.OrderHeader(),
                ListCarts = _db.Carts.Where(u => u.UserId == claim.Value).Include(p => p.Book.Category)
            };
            
            ShoppingCartVM.OrderHeader.Total = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser = _db.ApplicationUsers
                .FirstOrDefault(u => u.Id == claim.Value);

            foreach (var list in ShoppingCartVM.ListCarts)
            {
                list.Price = list.Book.Price;
                ShoppingCartVM.OrderHeader.Total += (list.Price * list.Count);
                if (list.Book.Description.Length > 100)
                {
                    list.Book.Description = list.Book.Description.Substring(0, 99) + "...";
                }
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int cartId)
        {
            var cart = _db.Carts.Include(p => p.Book).FirstOrDefault(c => c.Id == cartId);
            cart.Count += 1;
            cart.Price = cart.Book.Price;
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _db.Carts.Include(p => p.Book).FirstOrDefault(c => c.Id == cartId);

            if (cart.Count == 1)
            {
                var cnt = _db.Carts.Where(u => u.UserId == cart.UserId).ToList().Count;
                _db.Carts.Remove(cart);
                _db.SaveChanges();
                HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);
            }
            else
            {
                cart.Count -= 1;
                cart.Price = cart.Book.Price;
                _db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _db.Carts.Include(p => p.Book).FirstOrDefault(c => c.Id == cartId);

            var cnt = _db.Carts.Where(u => u.UserId == cart.UserId).ToList().Count;
            _db.Carts.Remove(cart);
            _db.SaveChanges();
            HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new Models.OrderHeader(),
                ListCarts = _db.Carts.Where(u => u.UserId == claim.Value).Include(a => a.Book)
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _db.ApplicationUsers
                .FirstOrDefault(u => u.Id == claim.Value);

            foreach (var list in ShoppingCartVM.ListCarts)
            {
                list.Price = list.Book.Price;
                ShoppingCartVM.OrderHeader.Total += (list.Price * list.Count);
            }

            ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.ApplicationUser.Address;
            ShoppingCartVM.OrderHeader.OderDate = DateTime.Now;

            return View(ShoppingCartVM);
        }

        // Hai cái function summary này đều ko nhận giá trị trả về nên nó đã vi phạm tính overload trong OOP (hai cái có
        // thể trùng tên nhưng phải khác giá trị truyền vào), nên chúng ta đặt một cái tên khác (SummaryPost) và để khi
        // post về nhận giá trị của function summary này thì chúng ta đặt một cái ActionName.
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost()
        {
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.OrderHeader.ApplicationUser = _db.ApplicationUsers.FirstOrDefault(c => c.Id == claim.Value);
            ShoppingCartVM.ListCarts = _db.Carts.Where(c => c.UserId == claim.Value).Include(a => a.Book);

            ShoppingCartVM.OrderHeader.UserId = claim.Value;
            ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.ApplicationUser.Address;
            ShoppingCartVM.OrderHeader.OderDate = DateTime.Now;

            // Chỗ này chúng ta tiến hành thêm vào và lưu OrderHeaders để xún dưới tạo OrderDetail sẽ có Id của OrderHeader 
            _db.OrderHeaders.Add(ShoppingCartVM.OrderHeader);
            _db.SaveChanges();

            foreach (var item in ShoppingCartVM.ListCarts)
            {
                item.Price = item.Book.Price;
                OderDetail orderDetails = new OderDetail()
                {
                    BookId = item.BookId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = item.Price,
                    Quantity = item.Count
                };
                
                ShoppingCartVM.OrderHeader.Total += orderDetails.Quantity * orderDetails.Price;
                _db.OderDetails.Add(orderDetails);
            }

            _db.Carts.RemoveRange(ShoppingCartVM.ListCarts);
            _db.SaveChanges();
            HttpContext.Session.SetInt32(SD.ssShoppingCart, 0);

            return RedirectToAction("OrderConfirmation", "Carts", new {id = ShoppingCartVM.OrderHeader.Id});
        }

        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
}