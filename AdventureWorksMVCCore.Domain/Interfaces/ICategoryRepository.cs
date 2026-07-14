using System.Collections.Generic;
using AdventureWorksMVCCore.Domain.Entities;

namespace AdventureWorksMVCCore.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Category entity operations
    /// </summary>
    public interface ICategoryRepository
    {
        IEnumerable<ProductCategory> GetAllWithSubcategories();
        ProductCategory GetById(int categoryId);
        ProductSubcategory GetSubcategoryById(int subcategoryId);
        IDictionary<int, ProductSubcategory> GetSubcategoryMap();
    }
}
