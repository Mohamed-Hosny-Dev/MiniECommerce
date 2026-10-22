using MiniECommerce.Data;
using MiniECommerce.Models;
using MiniECommerce.Repository.Interfaces;

namespace MiniECommerce.Repository.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IGenericRepository<Product> Products { get; }
        public IGenericRepository<Category> Categories { get; }
        public IGenericRepository<Order> Orders { get; }
        public IGenericRepository<OrderItem> OrderItems { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Products = new GenericRepository<Product>(_context);
            Categories = new GenericRepository<Category>(_context);
            Orders = new GenericRepository<Order>(_context);
            OrderItems = new GenericRepository<OrderItem>(_context);
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}