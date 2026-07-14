using System.Collections.Generic;
using System.Linq;
using AdventureWorksMVCCore.Application.Interfaces;
using AdventureWorksMVCCore.Domain.Entities;
using AdventureWorksMVCCore.Domain.Interfaces;

namespace AdventureWorksMVCCore.Application.Services
{
    /// <summary>
    /// Application service for product-related business logic
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public ProductSubcategory GetSubcategory(int subcategoryId)
        {
            return _categoryRepository.GetSubcategoryById(subcategoryId);
        }

        public List<Product> GetProductsBySubcategory(int subcategoryId)
        {
            return _productRepository.GetBySubcategory(subcategoryId).OrderBy(p => p.Name).ToList();
        }

        public Product GetProduct(int productId)
        {
            return _productRepository.GetById(productId);
        }

        public List<Product> Search(string query, int take = 60)
        {
            query = (query ?? "").Trim();
            if (query.Length == 0)
            {
                return new List<Product>();
            }
            return _productRepository.Search(query, take).ToList();
        }

        public IDictionary<int, ProductSubcategory> SubcategoryMap()
        {
            return _categoryRepository.GetSubcategoryMap();
        }
    }
}
