using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WebApiApp.Models;

namespace WebApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        ApplicationContext db;

        public AdminController(ApplicationContext context)
        {
            db = context;
        }

        [HttpGet("GetOrderById/{Id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Administrator")]
        public IActionResult GetOrderByUser(int Id)
        {
            var user = db.Users.FirstOrDefault(x => x.Id == Id);
            if (user != null)
            {
                var orders = db.Billings.Where(i => i.UserId == user.Id);
                return Ok(orders);
            }
            return NotFound();
        }

        [HttpGet("GetAllUsers")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Administrator")]
        public IActionResult GetAllUsers()
        {
            var users = db.Users.ToList();
            if (users.Count > 0)
            {
                return Ok(users);
            }
            return NotFound();
        }

        [HttpPost("CreateProduct")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Administrator")]
        public IActionResult CreateProduct(Product product)
        {
            var prod = db.Products.FirstOrDefault(p => p.Name == product.Name);

            if (prod == null)
            {
                db.Products.Add(new Product { Name = product.Name, Count = product.Count, Price = product.Price });
                db.SaveChanges();
                return Ok("Product created");
            }
            return Ok($"Product with this name: {product.Name} exists");
        }

        [HttpPost("CreateAccount")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Administrator")]
        public IActionResult CreateAccount(User user)
        {
            var newUser = db.Users.FirstOrDefault(x => x.Username == user.Username);
            if (newUser == null)
            {
                var passwordHasher = new PasswordHasher<User>();
                var hashedPassword = passwordHasher.HashPassword(user, user.Password);
                newUser = new User { Username = user.Username, EmailAddress = user.EmailAddress, GivenName = user.GivenName, Role = user.Role, Surname = user.Surname, Password = hashedPassword };
                db.Users.Add(newUser);
                db.SaveChanges();
                return Ok("Create account");
            }
            return Ok("Account created");
        }
    }
}
