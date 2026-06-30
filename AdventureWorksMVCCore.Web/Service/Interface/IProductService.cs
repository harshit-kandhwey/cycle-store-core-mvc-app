using System.Collections.Generic;
using AdventureWorksMVCCore.Web.Models;

namespace AdventureWorksMVCCore.Web.Service.Interface
{
    public interface IProductService
    {
        ProductSubcategory GetSubcategory(int subcategoryId);
        List<Product> GetProductsBySubcategory(int subcategoryId);
        Product GetProduct(int productId);

        // Search products by name or product number.
        List<Product> Search(string query, int take = 60);

        // All subcategories (with category) keyed by id — used to pick images for mixed result sets.
        IDictionary<int, ProductSubcategory> SubcategoryMap();
    }
}
