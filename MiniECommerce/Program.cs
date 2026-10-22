using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniECommerce.Data;
using MiniECommerce.Extensions;
using MiniECommerce.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC + global CSRF protection
builder.Services.AddControllersWithViews(options =>
{
    // Every POST/PUT/DELETE must carry a valid anti-forgery token. Forms get it automatically;
    // the AJAX code sends it in the "RequestVerificationToken" header (see site.js).
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

// 2. DbContext, repositories, application services, session
builder.Services.AddApplicationServices(builder.Configuration);

// 3. Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// 4. Database + seed data (roles, admin account, optional demo catalogue)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    if (app.Environment.IsDevelopment())
    {
        // Creates / updates the database automatically while developing.
        await services.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
    }

    await DbInitializer.SeedAsync(services, app.Configuration);
}

// 5. Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Friendly 404 / 403 pages instead of the browser's blank default
app.UseStatusCodePagesWithReExecute("/Home/Status", "?code={0}");

app.UseHttpsRedirection();

// The CSP is only enforced outside Development because the dev tools (dotnet watch / hot reload) inject inline scripts.
app.UseSecurityHeaders(enableCsp: !app.Environment.IsDevelopment());

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
