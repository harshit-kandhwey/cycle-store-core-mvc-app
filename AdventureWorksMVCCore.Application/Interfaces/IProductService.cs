using System.Collections.Generic;
using AdventureWorksMVCCore.Domain.Entities;

namespace AdventureWorksMVCCore.Application.Interfaces
{
    /// <summary>
    /// Application service interface for product operations
    /// </summary>
    public interface IProductService
    {
        ProductSubcategory GetSubcategory(int subcategoryId);
        List<Product> GetProductsBySubcategory(int subcategoryId);
        Product GetProduct(int productId);
        List<Product> Search(string query, int take = 60);
        IDictionary<int, ProductSubcategory> SubcategoryMap();
    }
}
