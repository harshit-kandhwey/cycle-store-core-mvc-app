using System.Collections.Generic;
using AdventureWorksMVCCore.Domain.Entities;

namespace AdventureWorksMVCCore.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Product entity operations
    /// </summary>
    public interface IProductRepository
    {
        Product GetById(int productId);
        IEnumerable<Product> GetBySubcategory(int subcategoryId);
        IEnumerable<Product> Search(string query, int take = 60);
        IEnumerable<Product> GetAll();
    }
}
