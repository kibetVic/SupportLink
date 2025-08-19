using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SupportLink.Data;
using SupportLink.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register DbContext
builder.Services.AddDbContext<SupportLinkDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Add Authentication (using cookies)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";          // Redirect if not logged in
        options.LogoutPath = "/Account/Logout";        // Logout endpoint
        options.AccessDeniedPath = "/Account/AccessDenied"; // If role not allowed
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// ✅ Add Session (optional)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Register HttpContextAccessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Ensure database is created and migrations are applied on startup
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<SupportLinkDbContext>();
//    db.Database.Migrate();

//    // Seed minimal data for Users and Organizations if empty
//    if (!db.Organizations.Any())
//    {
//        db.Organizations.Add(new Organization
//        {
//            OrganizationCode = "ORG-001",
//            OrganizationName = "Default Organization"
//        });
//        db.SaveChanges();
//    }

//    if (!db.Users.Any())
//    {
//        db.Users.Add(new AccountUser
//        {
//            UserName = "Default Staff",
//            Email = "staff@example.com",
//            Password = "Password123!",
//            Role = UserRole.Staff
//        });
//        db.SaveChanges();
//    }
//}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Authentication & Authorization middleware must come **before** endpoints
app.UseAuthentication();
app.UseAuthorization();

// ✅ Enable session middleware
app.UseSession();

// Default route (login first, then Home after login)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
