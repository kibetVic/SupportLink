using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportLink.Data;
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
        // Password hashing
        // ==========================
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        // ==========================
        // REGISTER
        // ==========================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(AccountUser model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // check if email already exists
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already registered!");
                return View(model);
            }

            // confirm password check
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match!");
                return View(model);
            }

            // hash password before saving
            model.Password = HashPassword(model.Password);
            model.ConfirmPassword = null; // ignored by EF anyway

            // 🚀 Force default role
            model.Role = UserRole.Staff;

            try
            {
                _context.Users.Add(model);
                _context.SaveChanges();

                TempData["Success"] = "Registration successful! Please login.";
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error saving user: " + ex.InnerException?.Message ?? ex.Message);
                return View(model);
            }
        }

        // ==========================
        // LOGIN
        // ==========================
        [HttpGet]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string hashed = HashPassword(model.Password);

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    (u.UserName == model.LoginIdentifier || u.Email == model.LoginIdentifier)
                    && u.Password == hashed);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, user.Role.ToString()), // use enum value
                    new Claim(ClaimTypes.Email, user.Email)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddMinutes(30) });

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid Username/Email or Password");
            return View(model);
        }

        // ==========================
        // LOGOUT
        // ==========================        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // ==========================
        // RESET PASSWORD
        // ==========================
        [HttpGet]
        public IActionResult ResetPassword() => View(new ResetPasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "No account found with that email.");
                return View(model);
            }

            user.Password = HashPassword(model.NewPassword);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Password reset successful! You can now log in.";
            return RedirectToAction("Login", "Account");
        }

        // ==========================
        // CRUD: AccountUser
        // ==========================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index() => View(await _context.Users.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var accountUser = await _context.Users.FirstOrDefaultAsync(m => m.UserId == id);
            if (accountUser == null) return NotFound();
            return View(accountUser);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(AccountUser accountUser)
        {
            if (ModelState.IsValid)
            {
                accountUser.Password = HashPassword(accountUser.Password);
                accountUser.ConfirmPassword = null;

                if (!Enum.IsDefined(typeof(UserRole), accountUser.Role))
                    accountUser.Role = UserRole.Staff;

                _context.Add(accountUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(accountUser);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var accountUser = await _context.Users.FindAsync(id);
            if (accountUser == null) return NotFound();
            return View(accountUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, AccountUser accountUser)
        {
            if (id != accountUser.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // prevent confirm password persistence
                    accountUser.ConfirmPassword = null;
                    _context.Update(accountUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.UserId == accountUser.UserId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(accountUser);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var accountUser = await _context.Users.FirstOrDefaultAsync(m => m.UserId == id);
            if (accountUser == null) return NotFound();
            return View(accountUser);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var accountUser = await _context.Users.FindAsync(id);
            if (accountUser != null) _context.Users.Remove(accountUser);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
