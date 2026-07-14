using System.Collections.Generic;
using System.Linq;
using AdventureWorksMVCCore.Application.Interfaces;
using AdventureWorksMVCCore.Domain.Entities;
using AdventureWorksMVCCore.Domain.Interfaces;

namespace AdventureWorksMVCCore.Application.Services
{
    /// <summary>
    /// Application service for category-related business logic
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public List<ProductCategory> GetCategoriesWithSubCategory()
        {
            return _categoryRepository.GetAllWithSubcategories().ToList();
        }
    }
}
