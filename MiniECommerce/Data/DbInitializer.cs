using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniECommerce.Models;

namespace MiniECommerce.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

            await SeedRolesAndAdminAsync(serviceProvider, configuration, logger);

            if (configuration.GetValue<bool>("Seed:DemoData"))
                await SeedDemoCatalogAsync(serviceProvider, logger);
        }

        private static async Task SeedRolesAndAdminAsync(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger logger)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // The admin credentials are no longer hard-coded in the source code.
            // Development: appsettings.Development.json.  Production: environment variables
            // (Seed__Admin__Email / Seed__Admin__Password) or user-secrets.
            var adminEmail = configuration["Seed:Admin:Email"];
            var adminPassword = configuration["Seed:Admin:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogWarning("Seed:Admin:Email / Seed:Admin:Password are not configured - admin account was not created.");
                return;
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    Email = adminEmail,
                    UserName = configuration["Seed:Admin:Name"] ?? "System Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                {
                    logger.LogError("Could not create the admin user: {Errors}",
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                    return;
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        /// <summary>Small sample catalogue so a fresh database doesn't give an empty-looking store.</summary>
        private static async Task SeedDemoCatalogAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (await context.Categories.AnyAsync() || await context.Products.AnyAsync())
                return;

            var electronics = new Category { Name = "Electronics", Description = "Audio, wearables and everyday gadgets" };
            var fashion = new Category { Name = "Fashion", Description = "Shoes, bags and accessories" };
            var home = new Category { Name = "Home & Kitchen", Description = "Make your home cosier" };
            var beauty = new Category { Name = "Beauty", Description = "Skin care and make-up" };

            context.Categories.AddRange(electronics, fashion, home, beauty);

            context.Products.AddRange(
                new Product { Name = "Wireless Headphones", Description = "Over-ear Bluetooth headphones with deep bass and 30 hours of battery life.", Price = 1299m, Stock = 25, ImageUrl = "/images/products/headphones.svg", Category = electronics },
                new Product { Name = "Smart Watch", Description = "Track your steps, heart rate and notifications right from your wrist.", Price = 2450m, Stock = 12, ImageUrl = "/images/products/smartwatch.svg", Category = electronics },
                new Product { Name = "Running Sneakers", Description = "Lightweight, breathable sneakers built for daily runs and long walks.", Price = 1150m, Stock = 40, ImageUrl = "/images/products/sneakers.svg", Category = fashion },
                new Product { Name = "Leather Crossbody Bag", Description = "Compact bag with an adjustable strap and plenty of room for essentials.", Price = 890m, Stock = 8, ImageUrl = "/images/products/bag.svg", Category = fashion },
                new Product { Name = "Coffee Maker", Description = "Brew a fresh pot in minutes with this stainless steel drip coffee maker.", Price = 1650m, Stock = 15, ImageUrl = "/images/products/coffee.svg", Category = home },
                new Product { Name = "Linen Throw Blanket", Description = "Soft, breathable throw for the sofa or the bed. Machine washable.", Price = 420m, Stock = 3, ImageUrl = "/images/products/blanket.svg", Category = home },
                new Product { Name = "Hydrating Face Serum", Description = "Lightweight daily serum that keeps your skin fresh and hydrated.", Price = 560m, Stock = 30, ImageUrl = "/images/products/serum.svg", Category = beauty },
                new Product { Name = "Matte Lipstick Set", Description = "A set of long-lasting matte shades in everyday colours.", Price = 340m, Stock = 0, ImageUrl = "/images/products/lipstick.svg", Category = beauty });

            await context.SaveChangesAsync();
            logger.LogInformation("Demo catalogue was added (Seed:DemoData = true).");
        }
    }
}
