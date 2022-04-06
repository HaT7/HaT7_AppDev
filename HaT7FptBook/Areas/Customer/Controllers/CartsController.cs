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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HaT7FptBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty] public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartsController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new Models.OrderHeader(),
                ListCarts = _db.Carts.Where(u => u.UserId == claim.Value).Include(p => p.Product)
            };
            ShoppingCartVM.OrderHeader.Total = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser = _db.ApplicationUsers
                .FirstOrDefault(u => u.Id == claim.Value);

            foreach (var list in ShoppingCartVM.ListCarts)
            {
                list.Price = list.Product.Price;
                ShoppingCartVM.OrderHeader.Total += (list.Price * list.Count);
                list.Product.Description = list.Product.Description;
                if (list.Product.Description.Length > 100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 99) + "...";
                }
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int cartId)
        {
            var cart = _db.Carts.Include(p => p.Product).FirstOrDefault(c => c.Id == cartId);
            cart.Count += 1;
            cart.Price = cart.Product.Price;
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        
        public IActionResult Minus(int cartId)
        {
            var cart = _db.Carts.Include(p => p.Product).FirstOrDefault(c => c.Id == cartId);
        
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
                cart.Price = cart.Product.Price;
                _db.SaveChanges();
            }
        
            return RedirectToAction(nameof(Index));
        }
        
        public IActionResult Remove(int cartId)
        {
            var cart = _db.Carts.Include(p => p.Product).FirstOrDefault(c => c.Id == cartId);

            var cnt = _db.Carts.Where(u => u.UserId == cart.UserId).ToList().Count;
            _db.Carts.Remove(cart);
            _db.SaveChanges();
            HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);
            return RedirectToAction(nameof(Index));
        }
        
        // public IActionResult Summary()
        // {
        //     var claimsIdentity = (ClaimsIdentity)User.Identity;
        //     var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        //     ShoppingCartVM = new ShoppingCartVM()
        //     {
        //         OrderHeader = new Models.OrderHeader(),
        //         ListCarts = _unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId==claim.Value, includeProperties:"Product")
        //     };
        //     ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
        //         .GetFirstOrDefault(u => u.Id == claim.Value, includeProperties: "company");
        //     foreach(var list in ShoppingCartVM.ListCarts)
        //     {
        //         list.Price = SD.GetPriceBaseOnQuanity(list.Count, list.Product.Price,
        //             list.Product.Price50, list.Product.Price100);
        //         ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
        //     }
        //
        //     ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
        //     ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
        //     ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
        //     ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
        //     ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
        //     ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
        //
        //     return View(ShoppingCartVM);
        // }

        // [HttpPost]
        // [ActionName("Summary")]
        // [ValidateAntiForgeryToken]
        // public IActionResult SummaryPost(string stripeToken)
        // {
        //     var claimsIdentity = (ClaimsIdentity)User.Identity;
        //     var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        //     ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
        //                                                     .GetFirstOrDefault(c => c.Id == claim.Value,
        //                                                             includeProperties: "company");
        //
        //     ShoppingCartVM.ListCarts = _unitOfWork.ShoppingCart
        //                                 .GetAll(c => c.ApplicationUserId == claim.Value,
        //                                 includeProperties:"Product");
        //
        //     ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
        //     ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
        //     ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;
        //     ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
        //
        //     _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
        //     _unitOfWork.Save();
        //
        //     foreach(var item in ShoppingCartVM.ListCarts)
        //     {
        //         item.Price = SD.GetPriceBaseOnQuanity(item.Count, item.Product.Price, 
        //             item.Product.Price50, item.Product.Price100);
        //         OrderDetails orderDetails = new OrderDetails()
        //         {
        //             ProductId = item.ProductId,
        //             OrderId = ShoppingCartVM.OrderHeader.Id,
        //             Price = item.Price,
        //             Count = item.Count
        //         };
        //         ShoppingCartVM.OrderHeader.OrderTotal += orderDetails.Count * orderDetails.Price;
        //         _unitOfWork.OrderDetails.Add(orderDetails);
        //         
        //     }
        //
        //     _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCarts);
        //     _unitOfWork.Save();
        //     HttpContext.Session.SetInt32(SD.ssShoppingCart, 0);
        //
        //     if (stripeToken == null)
        //     {
        //         //order will be created for delayed payment for authroized company
        //         ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
        //         ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
        //         ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
        //     }
        //     else
        //     {
        //         //process the payment
        //         var options = new ChargeCreateOptions
        //         {
        //             Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal * 100),
        //             Currency = "usd",
        //             Description = "Order ID : " + ShoppingCartVM.OrderHeader.Id,
        //             Source = stripeToken
        //         };
        //
        //         var service = new ChargeService();
        //         Charge charge = service.Create(options);
        //
        //         if (charge.Id == null)
        //         {
        //             ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
        //         }
        //         else
        //         {
        //             ShoppingCartVM.OrderHeader.TransactionId = charge.Id;
        //         }
        //         if (charge.Status.ToLower() == "succeeded")
        //         {
        //             ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
        //             ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
        //             ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
        //         }
        //     }
        //
        //     _unitOfWork.Save();
        //
        //     return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
        //
        // }

        // public IActionResult OrderConfirmation(int id)
        // {
        //     OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
        //     TwilioClient.Init(_twilioOptions.AccountSid, _twilioOptions.AuthToken);
        //     try
        //     {
        //         var message = MessageResource.Create(
        //             body: "Order placed in BookShop-MVC. Your Order Id:" + id,
        //             from: new Twilio.Types.PhoneNumber(_twilioOptions.PhoneNumber),
        //             to: new PhoneNumber(_twilioOptions.PhoneNumberTest)
        //             );
        //         
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);
        //         throw;
        //     }
        //     return View(id);
        // }
    }
}