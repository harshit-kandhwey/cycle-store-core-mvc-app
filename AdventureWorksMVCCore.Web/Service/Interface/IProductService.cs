using System.Collections.Generic;
using AdventureWorksMVCCore.Web.Models;

namespace AdventureWorksMVCCore.Web.Service.Interface
{
    public interface IProductService
    {
        ProductSubcategory GetSubcategory(int subcategoryId);
        List<Product> GetProductsBySubcategory(int subcategoryId);
        Product GetProduct(int productId);
    }
}
