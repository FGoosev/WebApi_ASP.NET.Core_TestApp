using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiApp.Models;

namespace WebApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _config;
        private ApplicationContext db;

        public LoginController(IConfiguration config, ApplicationContext context)
        {
            _config= config;
            db = context;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] UserLogin userLogin)
        {
            var user = Authenticate(userLogin);

            if (user != null)
            {
                var token = Generate(user);
                return Ok(token);
            }
            return NotFound();
        }

        private string Generate(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Email, user.EmailAddress),
                new Claim(ClaimTypes.GivenName, user.GivenName),
                new Claim(ClaimTypes.Surname, user.Surname),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private User Authenticate(UserLogin userLogin)
        {
            var passwordHasher = new PasswordHasher<User>();
            var currentUser = db.Users.FirstOrDefault(x => x.Username.ToLower() == userLogin.Username.ToLower());
            
            if (currentUser != null)
            {
                var result = passwordHasher.VerifyHashedPassword(currentUser, currentUser.Password,userLogin.Password);
                if (result == PasswordVerificationResult.Success)
                {
                    return currentUser;
                }
                return null;
                
            }
            return null;
        }
    }
}
