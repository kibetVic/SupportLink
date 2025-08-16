using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportLink.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SupportLink.Controllers
{
    public class AccountController : Controller
    {
        private readonly SupportLinkDbContext _context;

        public AccountController(SupportLinkDbContext context)
        {
            _context = context;
        }

        // ==========================
        // PASSWORD HASHING HELPER
        // ==========================
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        // ==========================
        // REGISTER
        // ==========================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(AccountUser model)
        {
            if (ModelState.IsValid)
            {
                // check if email already exists
                if (_context.ApplicationUsers.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("", "Email already registered!");
                    return View(model);
                }

                // check password confirmation
                if (model.Password != model.confirmPassword)
                {
                    ModelState.AddModelError("", "Passwords do not match!");
                    return View(model);
                }

                // hash password before saving
                model.Password = HashPassword(model.Password);
                model.confirmPassword = model.Password;

                // set default role = Staff
                model.Role = Role.Staff.ToString();

                _context.ApplicationUsers.Add(model);
                _context.SaveChanges();

                // redirect to login instead of auto login
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        // ==========================
        // LOGIN
        // ==========================
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel()); // ensure model is sent
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string hashed = HashPassword(model.Password);

            // Check by Email OR UserName
            var user = _context.ApplicationUsers
                .FirstOrDefault(u =>
                    (u.UserName == model.LoginIdentifier || u.Email == model.LoginIdentifier)
                    && u.Password == hashed);

            if (user != null)
            {
                // Store session
                HttpContext.Session.SetString("UserName", user.UserName);
                HttpContext.Session.SetString("UserRole", user.Role);

                // Redirect to Home
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid Username/Email or Password");
            return View(model);
        }

        // ==========================
        // LOGOUT
        // ==========================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
