using System.Collections.Generic;
using System.Linq;
using AdventureWorksMVCCore.Domain.Entities;
using AdventureWorksMVCCore.Domain.Interfaces;
using AdventureWorksMVCCore.Infrastructure.Data;

namespace AdventureWorksMVCCore.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Product entity
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly CycleStoreContext _context;

        public ProductRepository(CycleStoreContext context)
        {
            _context = context;
        }

        public Product GetById(int productId)
        {
            return _context.Product.FirstOrDefault(p => p.ProductId == productId);
        }

        public IEnumerable<Product> GetBySubcategory(int subcategoryId)
        {
            return _context.Product
                .Where(p => p.ProductSubcategoryId == subcategoryId)
                .OrderBy(p => p.Name)
                .ToList();
        }

        public IEnumerable<Product> Search(string query, int take = 60)
        {
            query = (query ?? "").Trim();
            if (query.Length == 0)
            {
                return new List<Product>();
            }
            return _context.Product
                .Where(p => p.Name.Contains(query) || p.ProductNumber.Contains(query))
                .OrderBy(p => p.Name)
                .Take(take)
                .ToList();
        }

        public IEnumerable<Product> GetAll()
        {
            return _context.Product.ToList();
        }
    }
}
