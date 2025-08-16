using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SupportLink.Models;
using System.Security.Claims;
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
                if (_context.ApplicationUsers.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("", "Email already registered!");
                    return View(model);
                }

                if (model.Password != model.confirmPassword)
                {
                    ModelState.AddModelError("", "Passwords do not match!");
                    return View(model);
                }

                model.Password = HashPassword(model.Password);
                model.confirmPassword = model.Password;
                model.Role = Role.Staff.ToString();

                _context.ApplicationUsers.Add(model);
                _context.SaveChanges();

                // ✅ After Register -> Login (not auto login)
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
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string hashed = HashPassword(model.Password);

            var user = _context.ApplicationUsers
                .FirstOrDefault(u =>
                    (u.UserName == model.LoginIdentifier || u.Email == model.LoginIdentifier)
                    && u.Password == hashed);

            if (user != null)
            {
                // ✅ Build claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                };

                // ✅ Sign in with cookie
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // ✅ Always go to Home after login
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid Username/Email or Password");
            return View(model);
        }

        // ==========================
        // LOGOUT
        // ==========================
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // ✅ Proper cookie sign-out
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Account");
        }
        // ==========================
        // RESET PASSWORD
        // ==========================
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View(new ResetPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.ApplicationUsers.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "No account found with that email.");
                return View(model);
            }

            // ✅ Hash and update password
            user.Password = HashPassword(model.NewPassword);
            user.confirmPassword = user.Password;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Password reset successful! You can now log in.";
            return RedirectToAction("Login", "Account");
        }
    }
}
