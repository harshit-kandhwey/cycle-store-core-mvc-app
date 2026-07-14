using System.Collections.Generic;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AdventureWorksMVCCore.Application.Interfaces;
using AdventureWorksMVCCore.Domain.Entities;
using AdventureWorksMVCCore.Web.Controllers;

namespace AdventureWorksMVCCore.Tests.Controllers
{
    /// <summary>
    /// Unit tests for ProductsController
    /// </summary>
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _mockCategoryService = new Mock<ICategoryService>();
            _controller = new ProductsController(_mockProductService.Object, _mockCategoryService.Object);
        }

        [Fact]
        public void Details_WithValidId_ReturnsViewResult()
        {
            // Arrange
            var productId = 1;
            var product = new Product
            {
                ProductId = productId,
                Name = "Test Product",
                ProductNumber = "TEST-001",
                ListPrice = 99.99m
            };
            _mockProductService.Setup(s => s.GetProduct(productId)).Returns(product);

            // Act
            var result = _controller.Details(productId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Product>(viewResult.Model);
            Assert.Equal(productId, model.ProductId);
            Assert.Equal("Test Product", model.Name);
        }

        [Fact]
        public void Details_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var productId = 999;
            _mockProductService.Setup(s => s.GetProduct(productId)).Returns((Product)null);

            // Act
            var result = _controller.Details(productId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Search_WithValidQuery_ReturnsViewWithResults()
        {
            // Arrange
            var query = "bike";
            var products = new List<Product>
            {
                new Product { ProductId = 1, Name = "Mountain Bike", ProductNumber = "BK-M001", ListPrice = 500m },
                new Product { ProductId = 2, Name = "Road Bike", ProductNumber = "BK-R001", ListPrice = 600m }
            };
            _mockProductService.Setup(s => s.Search(query, 60)).Returns(products);
            _mockProductService.Setup(s => s.SubcategoryMap()).Returns(new Dictionary<int, ProductSubcategory>());

            // Act
            var result = _controller.Search(query);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            Assert.Equal(query, _controller.ViewBag.Query);
        }

        [Fact]
        public void Search_WithEmptyQuery_ReturnsViewWithEmptyResults()
        {
            // Arrange
            var query = "";
            _mockProductService.Setup(s => s.SubcategoryMap()).Returns(new Dictionary<int, ProductSubcategory>());

            // Act
            var result = _controller.Search(query);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
        }

        [Fact]
        public void Search_WithLongQuery_TruncatesTo100Characters()
        {
            // Arrange
            var longQuery = new string('a', 150);
            _mockProductService.Setup(s => s.SubcategoryMap()).Returns(new Dictionary<int, ProductSubcategory>());

            // Act
            var result = _controller.Search(longQuery);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            // Query should be truncated to 100 characters
            Assert.Equal(100, ((string)_controller.ViewBag.Query).Length);
        }

        [Fact]
        public void Category_WithValidCategoryName_ReturnsViewResult()
        {
            // Arrange
            var categoryName = "Bikes";
            var category = new ProductCategory
            {
                ProductCategoryId = 1,
                Name = categoryName,
                ProductSubcategory = new List<ProductSubcategory>
                {
                    new ProductSubcategory { ProductSubcategoryId = 1, Name = "Mountain Bikes" }
                }
            };
            var categories = new List<ProductCategory> { category };
            _mockCategoryService.Setup(s => s.GetCategoriesWithSubCategory()).Returns(categories);
            _mockProductService.Setup(s => s.GetProductsBySubcategory(It.IsAny<int>())).Returns(new List<Product>());

            // Act
            var result = _controller.Category(categoryName);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
        }

        [Fact]
        public void Category_WithInvalidCategoryName_ReturnsNotFound()
        {
            // Arrange
            var categoryName = "InvalidCategory";
            _mockCategoryService.Setup(s => s.GetCategoriesWithSubCategory()).Returns(new List<ProductCategory>());

            // Act
            var result = _controller.Category(categoryName);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Subcategory_WithValidId_ReturnsViewResult()
        {
            // Arrange
            var subcategoryId = 1;
            var subcategory = new ProductSubcategory
            {
                ProductSubcategoryId = subcategoryId,
                Name = "Mountain Bikes"
            };
            var products = new List<Product>
            {
                new Product { ProductId = 1, Name = "Test Bike", ProductNumber = "BK-001", ListPrice = 500m }
            };
            _mockProductService.Setup(s => s.GetSubcategory(subcategoryId)).Returns(subcategory);
            _mockProductService.Setup(s => s.GetProductsBySubcategory(subcategoryId)).Returns(products);

            // Act
            var result = _controller.Subcategory(subcategoryId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
        }

        [Fact]
        public void Subcategory_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var subcategoryId = 999;
            _mockProductService.Setup(s => s.GetSubcategory(subcategoryId)).Returns((ProductSubcategory)null);

            // Act
            var result = _controller.Subcategory(subcategoryId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
