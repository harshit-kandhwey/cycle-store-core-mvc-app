using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;
using AdventureWorksMVCCore.Application.Services;
using AdventureWorksMVCCore.Domain.Entities;
using AdventureWorksMVCCore.Domain.Interfaces;

namespace AdventureWorksMVCCore.Tests.Services
{
    /// <summary>
    /// Unit tests for CategoryService
    /// </summary>
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepo;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _mockCategoryRepo = new Mock<ICategoryRepository>();
            _categoryService = new CategoryService(_mockCategoryRepo.Object);
        }

        [Fact]
        public void GetCategoriesWithSubCategory_ReturnsAllCategories()
        {
            // Arrange
            var categories = new List<ProductCategory>
            {
                new ProductCategory
                {
                    ProductCategoryId = 1,
                    Name = "Bikes",
                    ProductSubcategory = new List<ProductSubcategory>
                    {
                        new ProductSubcategory { ProductSubcategoryId = 1, Name = "Mountain Bikes" },
                        new ProductSubcategory { ProductSubcategoryId = 2, Name = "Road Bikes" }
                    }
                },
                new ProductCategory
                {
                    ProductCategoryId = 2,
                    Name = "Components",
                    ProductSubcategory = new List<ProductSubcategory>
                    {
                        new ProductSubcategory { ProductSubcategoryId = 3, Name = "Handlebars" }
                    }
                }
            };
            _mockCategoryRepo.Setup(r => r.GetAllWithSubcategories()).Returns(categories);

            // Act
            var result = _categoryService.GetCategoriesWithSubCategory();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Bikes", result[0].Name);
            Assert.Equal(2, result[0].ProductSubcategory.Count);
            Assert.Equal("Components", result[1].Name);
            Assert.Single(result[1].ProductSubcategory);
            _mockCategoryRepo.Verify(r => r.GetAllWithSubcategories(), Times.Once);
        }

        [Fact]
        public void GetCategoriesWithSubCategory_WhenEmpty_ReturnsEmptyList()
        {
            // Arrange
            _mockCategoryRepo.Setup(r => r.GetAllWithSubcategories()).Returns(new List<ProductCategory>());

            // Act
            var result = _categoryService.GetCategoriesWithSubCategory();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockCategoryRepo.Verify(r => r.GetAllWithSubcategories(), Times.Once);
        }
    }
}
