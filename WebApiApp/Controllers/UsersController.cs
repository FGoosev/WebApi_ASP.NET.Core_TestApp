
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Security.Claims;
using System.Text.Json.Serialization;
using WebApiApp.Models;


namespace WebApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        ApplicationContext db;

        public UsersController(ApplicationContext context)
        {
            db = context;
        }
        private User GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var userClaims = identity.Claims;

                return new User
                {
                    Username = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value,
                    EmailAddress = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value,
                    GivenName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.GivenName)?.Value,
                    Surname = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Surname)?.Value,
                    Role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value
                };
            }
            return null;
        }

        [HttpPost("CreateBill")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Seller")]
        public IActionResult CreateBill()
        {
            decimal total = 0;
            var currentUser = GetCurrentUser();
            var user = db.Users.FirstOrDefault(x => x.Username == currentUser.Username);
            var carts = db.Carts.Where(x => x.BillingId == 0).Include(x => x.Product).ToList();

            if (user != null)
            {
                if (carts.Count > 0)
                {
                    foreach (var cart in carts)
                    {
                        total += cart.Count * cart.Product.Price;
                    }
                    db.Billings.Add(new Billing { UserId = user.Id, DateOrder = DateTime.Today, Total = total });
                    db.SaveChanges();


                    var billing = db.Billings.Include(u => u.User).OrderByDescending(b => b).FirstOrDefault(x => x.UserId == user.Id);


                    foreach (var cart in carts)
                    {
                        cart.BillingId = billing.Id;
                        db.Carts.Update(cart);
                        db.SaveChanges();
                    }
                    return Ok("Bill created");
                }
                return NotFound();

            }
            return NotFound();

        }

        [HttpPost("CreateCart")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Seller")]
        public IActionResult AddCart(Cart cart)
        {
            var user = db.Users.FirstOrDefault(x => x.Id == cart.UserId);
            var product = db.Products.FirstOrDefault(p => p.Id == cart.ProductId);
            if (user != null && product != null)
            {
                db.Carts.Add(new Cart { ProductId = cart.ProductId, UserId = cart.UserId, Count = cart.Count, BillingId = 0 });
                db.SaveChanges();
                return Ok("Bill created");
            }
            return Ok("User or product not found");
        }


        [HttpGet("GetMyOrders")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Seller")]
        public IActionResult GetOrders()
        {
            var currentUser = GetCurrentUser();
            var user = db.Users.FirstOrDefault(x => x.Username == currentUser.Username);
            if (user != null)
            {
                var bills = db.Billings.Where(x => x.UserId == user.Id).ToList();
                if (bills.Count > 0)
                {
                    foreach (var bill in bills)
                    {
                        var carts = db.Carts.Include(p => p.Product);
                        return Ok(carts);
                    }
                }
                return Ok("Bill not found");
            }
            return NotFound();
        }
    }
}