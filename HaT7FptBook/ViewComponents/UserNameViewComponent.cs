using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HaT7FptBook.Data;
using Microsoft.AspNetCore.Mvc;

namespace HaT7FptBook.ViewComponents
{
    public class UserNameViewComponent: ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public UserNameViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var userFromdb = _db.ApplicationUsers.FirstOrDefault(a => a.Id == claims.Value);
            return View(userFromdb);
        }
    }
}