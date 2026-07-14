using System;
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
    /// Unit tests for ProductService
    /// </summary>
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepo;
        private readonly Mock<ICategoryRepository> _mockCategoryRepo;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockProductRepo = new Mock<IProductRepository>();
            _mockCategoryRepo = new Mock<ICategoryRepository>();
            _productService = new ProductService(_mockProductRepo.Object, _mockCategoryRepo.Object);
        }

        [Fact]
        public void GetProduct_WithValidId_ReturnsProduct()
        {
            // Arrange
            var productId = 1;
            var expectedProduct = new Product
            {
                ProductId = productId,
                Name = "Test Product",
                ProductNumber = "TEST-001",
                ListPrice = 99.99m
            };
            _mockProductRepo.Setup(r => r.GetById(productId)).Returns(expectedProduct);

            // Act
            var result = _productService.GetProduct(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.ProductId);
            Assert.Equal("Test Product", result.Name);
            _mockProductRepo.Verify(r => r.GetById(productId), Times.Once);
        }

        [Fact]
        public void GetProduct_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var productId = 999;
            _mockProductRepo.Setup(r => r.GetById(productId)).Returns((Product)null);

            // Act
            var result = _productService.GetProduct(productId);

            // Assert
            Assert.Null(result);
            _mockProductRepo.Verify(r => r.GetById(productId), Times.Once);
        }

        [Fact]
        public void GetProductsBySubcategory_ReturnsOrderedProducts()
        {
            // Arrange
            var subcategoryId = 1;
            var products = new List<Product>
            {
                new Product { ProductId = 3, Name = "Zebra Product", ProductNumber = "Z-001", ListPrice = 50m },
                new Product { ProductId = 1, Name = "Alpha Product", ProductNumber = "A-001", ListPrice = 100m },
                new Product { ProductId = 2, Name = "Beta Product", ProductNumber = "B-001", ListPrice = 75m }
            };
            _mockProductRepo.Setup(r => r.GetBySubcategory(subcategoryId)).Returns(products);

            // Act
            var result = _productService.GetProductsBySubcategory(subcategoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("Alpha Product", result[0].Name); // Should be ordered by name
            Assert.Equal("Beta Product", result[1].Name);
            Assert.Equal("Zebra Product", result[2].Name);
        }

        [Fact]
        public void Search_WithValidQuery_ReturnsMatchingProducts()
        {
            // Arrange
            var query = "bike";
            var products = new List<Product>
            {
                new Product { ProductId = 1, Name = "Mountain Bike", ProductNumber = "BK-M001", ListPrice = 500m },
                new Product { ProductId = 2, Name = "Road Bike", ProductNumber = "BK-R001", ListPrice = 600m }
            };
            _mockProductRepo.Setup(r => r.Search(query, 60)).Returns(products);

            // Act
            var result = _productService.Search(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _mockProductRepo.Verify(r => r.Search(query, 60), Times.Once);
        }

        [Fact]
        public void Search_WithEmptyQuery_ReturnsEmptyList()
        {
            // Arrange
            var query = "";

            // Act
            var result = _productService.Search(query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockProductRepo.Verify(r => r.Search(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Search_WithNullQuery_ReturnsEmptyList()
        {
            // Arrange
            string query = null;

            // Act
            var result = _productService.Search(query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockProductRepo.Verify(r => r.Search(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void GetSubcategory_WithValidId_ReturnsSubcategory()
        {
            // Arrange
            var subcategoryId = 1;
            var expectedSubcategory = new ProductSubcategory
            {
                ProductSubcategoryId = subcategoryId,
                Name = "Mountain Bikes",
                ProductCategoryId = 1
            };
            _mockCategoryRepo.Setup(r => r.GetSubcategoryById(subcategoryId)).Returns(expectedSubcategory);

            // Act
            var result = _productService.GetSubcategory(subcategoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(subcategoryId, result.ProductSubcategoryId);
            Assert.Equal("Mountain Bikes", result.Name);
        }

        [Fact]
        public void SubcategoryMap_ReturnsDictionary()
        {
            // Arrange
            var subcategories = new Dictionary<int, ProductSubcategory>
            {
                { 1, new ProductSubcategory { ProductSubcategoryId = 1, Name = "Mountain Bikes" } },
                { 2, new ProductSubcategory { ProductSubcategoryId = 2, Name = "Road Bikes" } }
            };
            _mockCategoryRepo.Setup(r => r.GetSubcategoryMap()).Returns(subcategories);

            // Act
            var result = _productService.SubcategoryMap();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey(1));
            Assert.True(result.ContainsKey(2));
        }
    }
}
