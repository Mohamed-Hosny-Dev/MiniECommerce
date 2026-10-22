using Microsoft.EntityFrameworkCore;
using MiniECommerce.Data;
using MiniECommerce.Repository.Implementation;
using MiniECommerce.Repository.Interfaces;
using MiniECommerce.Services.Implementation;
using MiniECommerce.Services.Interfaces;

namespace MiniECommerce.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            // DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Application services
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IProfileService, ProfileService>();

            // Session (the cart lives here). Session needs a distributed cache; the in-memory one is fine for a single server.
            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.Name = ".OurStore.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            return services;
        }
    }
}
