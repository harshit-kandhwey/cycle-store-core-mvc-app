using System.Collections.Generic;
using AdventureWorksMVCCore.Domain.Entities;

namespace AdventureWorksMVCCore.Application.Interfaces
{
    /// <summary>
    /// Application service interface for category operations
    /// </summary>
    public interface ICategoryService
    {
        List<ProductCategory> GetCategoriesWithSubCategory();
    }
}
