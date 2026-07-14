using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AdventureWorksMVCCore.Domain.Entities;
using AdventureWorksMVCCore.Domain.Interfaces;
using AdventureWorksMVCCore.Infrastructure.Data;

namespace AdventureWorksMVCCore.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Category entity
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CycleStoreContext _context;

        public CategoryRepository(CycleStoreContext context)
        {
            _context = context;
        }

        public IEnumerable<ProductCategory> GetAllWithSubcategories()
        {
            return _context.ProductCategory
                .Include(c => c.ProductSubcategory)
                .ToList();
        }

        public ProductCategory GetById(int categoryId)
        {
            return _context.ProductCategory
                .Include(c => c.ProductSubcategory)
                .FirstOrDefault(c => c.ProductCategoryId == categoryId);
        }

        public ProductSubcategory GetSubcategoryById(int subcategoryId)
        {
            return _context.ProductSubcategory
                .Include(s => s.ProductCategory)
                .FirstOrDefault(s => s.ProductSubcategoryId == subcategoryId);
        }

        public IDictionary<int, ProductSubcategory> GetSubcategoryMap()
        {
            return _context.ProductSubcategory
                .Include(s => s.ProductCategory)
                .ToDictionary(s => s.ProductSubcategoryId, s => s);
        }
    }
}
