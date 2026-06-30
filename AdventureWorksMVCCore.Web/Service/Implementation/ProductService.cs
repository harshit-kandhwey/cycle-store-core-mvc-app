using System.Collections.Generic;
using System.Linq;
using AdventureWorksMVCCore.Web.Models;
using AdventureWorksMVCCore.Web.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorksMVCCore.Web.Service.Implementation
{
    // Application-tier business logic: turns category/product browse requests into
    // EF Core queries against the SQL Server database (the DB tier does the filtering).
    public class ProductService : IProductService
    {
        private readonly CYCLE_STOREContext _context;

        public ProductService(CYCLE_STOREContext context)
        {
            _context = context;
        }

        public ProductSubcategory GetSubcategory(int subcategoryId)
        {
            return _context.ProductSubcategory
                .Include(s => s.ProductCategory)
                .FirstOrDefault(s => s.ProductSubcategoryId == subcategoryId);
        }

        public List<Product> GetProductsBySubcategory(int subcategoryId)
        {
            return _context.Product
                .Where(p => p.ProductSubcategoryId == subcategoryId)
                .OrderBy(p => p.Name)
                .ToList();
        }

        public Product GetProduct(int productId)
        {
            return _context.Product.FirstOrDefault(p => p.ProductId == productId);
        }
    }
}
