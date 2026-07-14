# ASP.NET MVC to .NET 8 Core MVC Migration Guide
## Clean Architecture Implementation

This document provides a comprehensive guide for migrating from legacy ASP.NET MVC (3/4/5) to .NET 8 Core MVC with Clean Architecture.

## Table of Contents

1. [Overview](#overview)
2. [Architecture Transformation](#architecture-transformation)
3. [Migration Steps](#migration-steps)
4. [Key Changes](#key-changes)
5. [Testing Strategy](#testing-strategy)
6. [Deployment](#deployment)

## Overview

### Source Application
- **Framework**: ASP.NET MVC 5 on .NET Framework 4.x
- **Architecture**: Monolithic MVC application
- **Data Access**: Entity Framework 6
- **Structure**: Single project with Models, Views, Controllers

### Target Application
- **Framework**: ASP.NET Core MVC on .NET 8
- **Architecture**: Clean Architecture with 4 layers
- **Data Access**: Entity Framework Core 8.0
- **Structure**: Multi-project solution with clear separation of concerns

## Architecture Transformation

### Before: Monolithic MVC Structure
```
AdventureWorksMVC/
├── Controllers/
├── Models/
├── Views/
├── App_Start/
├── Content/
├── Scripts/
└── Global.asax
```

### After: Clean Architecture Structure
```
AdventureWorksMVCCore/
├── AdventureWorksMVCCore.Domain/          # Core business entities
│   ├── Entities/
│   └── Interfaces/
├── AdventureWorksMVCCore.Application/     # Business logic
│   ├── Services/
│   ├── Interfaces/
│   └── DTOs/
├── AdventureWorksMVCCore.Infrastructure/  # Data access
│   ├── Data/
│   └── Repositories/
├── AdventureWorksMVCCore.Web/             # Presentation
│   ├── Controllers/
│   ├── Views/
│   ├── ViewModels/
│   └── wwwroot/
└── AdventureWorksMVCCore.Tests/           # Unit tests
    ├── Services/
    └── Controllers/
```

## Migration Steps

### Phase 1: Project Setup (Week 1)

#### 1.1 Create Solution Structure
```bash
# Create solution
dotnet new sln -n AdventureWorksMVCCore

# Create Domain project
dotnet new classlib -n AdventureWorksMVCCore.Domain -f net8.0
dotnet sln add AdventureWorksMVCCore.Domain

# Create Application project
dotnet new classlib -n AdventureWorksMVCCore.Application -f net8.0
dotnet sln add AdventureWorksMVCCore.Application

# Create Infrastructure project
dotnet new classlib -n AdventureWorksMVCCore.Infrastructure -f net8.0
dotnet sln add AdventureWorksMVCCore.Infrastructure

# Create Web project
dotnet new mvc -n AdventureWorksMVCCore.Web -f net8.0
dotnet sln add AdventureWorksMVCCore.Web

# Create Test project
dotnet new xunit -n AdventureWorksMVCCore.Tests -f net8.0
dotnet sln add AdventureWorksMVCCore.Tests
```

#### 1.2 Add Project References
```bash
# Application depends on Domain
cd AdventureWorksMVCCore.Application
dotnet add reference ../AdventureWorksMVCCore.Domain

# Infrastructure depends on Domain
cd ../AdventureWorksMVCCore.Infrastructure
dotnet add reference ../AdventureWorksMVCCore.Domain

# Web depends on Application and Infrastructure
cd ../AdventureWorksMVCCore.Web
dotnet add reference ../AdventureWorksMVCCore.Application
dotnet add reference ../AdventureWorksMVCCore.Infrastructure

# Tests depend on all projects
cd ../AdventureWorksMVCCore.Tests
dotnet add reference ../AdventureWorksMVCCore.Domain
dotnet add reference ../AdventureWorksMVCCore.Application
dotnet add reference ../AdventureWorksMVCCore.Infrastructure
dotnet add reference ../AdventureWorksMVCCore.Web
```

#### 1.3 Install NuGet Packages

**Infrastructure Project:**
```bash
cd AdventureWorksMVCCore.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
```

**Test Project:**
```bash
cd AdventureWorksMVCCore.Tests
dotnet add package xunit --version 2.6.2
dotnet add package xunit.runner.visualstudio --version 2.5.4
dotnet add package Moq --version 4.20.70
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 8.0.0
```

### Phase 2: Domain Layer Migration (Week 1-2)

#### 2.1 Create Domain Entities

**Product.cs** (Domain/Entities/)
```csharp
using System;

namespace AdventureWorksMVCCore.Domain.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string ProductNumber { get; set; }
        public decimal ListPrice { get; set; }
        public string Color { get; set; }
        public int? ProductSubcategoryId { get; set; }
        public DateTime SellStartDate { get; set; }
        public DateTime? SellEndDate { get; set; }
        
        // Navigation properties
        public virtual ProductSubcategory ProductSubcategory { get; set; }
    }
}
```

#### 2.2 Create Repository Interfaces

**IProductRepository.cs** (Domain/Interfaces/)
```csharp
using System.Collections.Generic;
using AdventureWorksMVCCore.Domain.Entities;

namespace AdventureWorksMVCCore.Domain.Interfaces
{
    public interface IProductRepository
    {
        Product GetById(int productId);
        IEnumerable<Product> GetBySubcategory(int subcategoryId);
        IEnumerable<Product> Search(string query, int take = 60);
        IEnumerable<Product> GetAll();
    }
}
```

### Phase 3: Infrastructure Layer Migration (Week 2)

#### 3.1 Create DbContext

**CycleStoreContext.cs** (Infrastructure/Data/)
```csharp
using Microsoft.EntityFrameworkCore;
using AdventureWorksMVCCore.Domain.Entities;

namespace AdventureWorksMVCCore.Infrastructure.Data
{
    public class CycleStoreContext : DbContext
    {
        public CycleStoreContext(DbContextOptions<CycleStoreContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Product { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<ProductSubcategory> ProductSubcategory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure entity mappings
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product", "Production");
                entity.Property(e => e.ProductId).HasColumnName("ProductID");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                // ... additional configurations
            });
        }
    }
}
```

#### 3.2 Implement Repositories

**ProductRepository.cs** (Infrastructure/Repositories/)
```csharp
using System.Collections.Generic;
using System.Linq;
using AdventureWorksMVCCore.Domain.Entities;
using AdventureWorksMVCCore.Domain.Interfaces;
using AdventureWorksMVCCore.Infrastructure.Data;

namespace AdventureWorksMVCCore.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly CycleStoreContext _context;

        public ProductRepository(CycleStoreContext context)
        {
            _context = context;
        }

        public Product GetById(int productId)
        {
            return _context.Product.FirstOrDefault(p => p.ProductId == productId);
        }

        public IEnumerable<Product> GetBySubcategory(int subcategoryId)
        {
            return _context.Product
                .Where(p => p.ProductSubcategoryId == subcategoryId)
                .OrderBy(p => p.Name)
                .ToList();
        }

        public IEnumerable<Product> Search(string query, int take = 60)
        {
            return _context.Product
                .Where(p => p.Name.Contains(query) || p.ProductNumber.Contains(query))
                .OrderBy(p => p.Name)
                .Take(take)
                .ToList();
        }

        public IEnumerable<Product> GetAll()
        {
            return _context.Product.ToList();
        }
    }
}
```

### Phase 4: Application Layer Migration (Week 2-3)

#### 4.1 Create Service Interfaces

**IProductService.cs** (Application/Interfaces/)
```csharp
using System.Collections.Generic;
using AdventureWorksMVCCore.Domain.Entities;

namespace AdventureWorksMVCCore.Application.Interfaces
{
    public interface IProductService
    {
        Product GetProduct(int productId);
        List<Product> GetProductsBySubcategory(int subcategoryId);
        List<Product> Search(string query, int take = 60);
        ProductSubcategory GetSubcategory(int subcategoryId);
        IDictionary<int, ProductSubcategory> SubcategoryMap();
    }
}
```

#### 4.2 Implement Services

**ProductService.cs** (Application/Services/)
```csharp
using System.Collections.Generic;
using System.Linq;
using AdventureWorksMVCCore.Application.Interfaces;
using AdventureWorksMVCCore.Domain.Entities;
using AdventureWorksMVCCore.Domain.Interfaces;

namespace AdventureWorksMVCCore.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public Product GetProduct(int productId)
        {
            return _productRepository.GetById(productId);
        }

        public List<Product> GetProductsBySubcategory(int subcategoryId)
        {
            return _productRepository.GetBySubcategory(subcategoryId)
                .OrderBy(p => p.Name)
                .ToList();
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

        public ProductSubcategory GetSubcategory(int subcategoryId)
        {
            return _categoryRepository.GetSubcategoryById(subcategoryId);
        }

        public IDictionary<int, ProductSubcategory> SubcategoryMap()
        {
            return _categoryRepository.GetSubcategoryMap();
        }
    }
}
```

### Phase 5: Presentation Layer Migration (Week 3-4)

#### 5.1 Update Startup.cs

**Startup.cs** (Web/)
```csharp
using AdventureWorksMVCCore.Application.Interfaces;
using AdventureWorksMVCCore.Application.Services;
using AdventureWorksMVCCore.Domain.Interfaces;
using AdventureWorksMVCCore.Infrastructure.Data;
using AdventureWorksMVCCore.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();

        // Register DbContext
        services.AddDbContext<CycleStoreContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

        // Register repositories (Infrastructure layer)
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // Register services (Application layer)
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();

        // Session support
        services.AddSession();
        services.AddHttpContextAccessor();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseSession();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}
```

#### 5.2 Update Controllers

**ProductsController.cs** (Web/Controllers/)
```csharp
using Microsoft.AspNetCore.Mvc;
using AdventureWorksMVCCore.Application.Interfaces;
using AdventureWorksMVCCore.Web.ViewModels;

namespace AdventureWorksMVCCore.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var product = _productService.GetProduct(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpGet]
        public IActionResult Search(string q)
        {
            var results = _productService.Search(q);
            return View(results);
        }
    }
}
```

### Phase 6: Testing Implementation (Week 4)

#### 6.1 Service Unit Tests

**ProductServiceTests.cs** (Tests/Services/)
```csharp
using Xunit;
using Moq;
using AdventureWorksMVCCore.Application.Services;
using AdventureWorksMVCCore.Domain.Interfaces;

public class ProductServiceTests
{
    [Fact]
    public void GetProduct_WithValidId_ReturnsProduct()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockCategoryRepo = new Mock<ICategoryRepository>();
        var service = new ProductService(mockRepo.Object, mockCategoryRepo.Object);
        
        var product = new Product { ProductId = 1, Name = "Test" };
        mockRepo.Setup(r => r.GetById(1)).Returns(product);

        // Act
        var result = service.GetProduct(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ProductId);
    }
}
```

#### 6.2 Controller Unit Tests

**ProductsControllerTests.cs** (Tests/Controllers/)
```csharp
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using AdventureWorksMVCCore.Web.Controllers;
using AdventureWorksMVCCore.Application.Interfaces;

public class ProductsControllerTests
{
    [Fact]
    public void Details_WithValidId_ReturnsViewResult()
    {
        // Arrange
        var mockService = new Mock<IProductService>();
        var mockCategoryService = new Mock<ICategoryService>();
        var controller = new ProductsController(mockService.Object, mockCategoryService.Object);
        
        var product = new Product { ProductId = 1, Name = "Test" };
        mockService.Setup(s => s.GetProduct(1)).Returns(product);

        // Act
        var result = controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
    }
}
```

## Key Changes

### 1. Configuration Migration

**Before (web.config):**
```xml
<configuration>
  <connectionStrings>
    <add name="DefaultConnection" connectionString="..." />
  </connectionStrings>
  <appSettings>
    <add key="Setting1" value="Value1" />
  </appSettings>
</configuration>
```

**After (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;"
  },
  "AppSettings": {
    "Setting1": "Value1"
  }
}
```

### 2. Dependency Injection

**Before (Global.asax):**
```csharp
public class MvcApplication : System.Web.HttpApplication
{
    protected void Application_Start()
    {
        // Manual dependency setup
        DependencyResolver.SetResolver(new CustomDependencyResolver());
    }
}
```

**After (Startup.cs):**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Built-in DI container
    services.AddScoped<IProductService, ProductService>();
    services.AddScoped<IProductRepository, ProductRepository>();
}
```

### 3. Entity Framework Migration

**Before (EF6):**
```csharp
public class MyContext : DbContext
{
    public MyContext() : base("DefaultConnection")
    {
    }
}
```

**After (EF Core 8):**
```csharp
public class CycleStoreContext : DbContext
{
    public CycleStoreContext(DbContextOptions<CycleStoreContext> options)
        : base(options)
    {
    }
}
```

### 4. Controller Actions

**Before:**
```csharp
public ActionResult Details(int id)
{
    var product = db.Products.Find(id);
    return View(product);
}
```

**After:**
```csharp
[HttpGet]
public IActionResult Details(int id)
{
    var product = _productService.GetProduct(id);
    if (product == null)
    {
        return NotFound();
    }
    return View(product);
}
```

## Testing Strategy

### Unit Testing Approach

1. **Service Layer Tests**: Test business logic with mocked repositories
2. **Controller Tests**: Test HTTP behavior with mocked services
3. **Repository Tests**: Integration tests with test database

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test class
dotnet test --filter ProductServiceTests
```

## Deployment

### Build and Publish

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build --configuration Release

# Publish web application
dotnet publish AdventureWorksMVCCore.Web -c Release -o ./publish
```

### Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdventureWorksMVCCore.Web.dll"]
```

## Success Criteria

✅ All projects build successfully  
✅ All unit tests pass  
✅ Application runs without errors  
✅ Database connectivity works  
✅ All features function as expected  
✅ Performance meets requirements  

## Conclusion

This migration transforms a monolithic ASP.NET MVC application into a modern, maintainable .NET 8 application with Clean Architecture. The layered approach provides:

- **Better testability** through dependency injection
- **Improved maintainability** with clear separation of concerns
- **Enhanced flexibility** for future changes
- **Modern technology stack** with .NET 8 and EF Core 8

For questions or issues, refer to the official Microsoft documentation:
- [ASP.NET Core MVC](https://docs.microsoft.com/aspnet/core/mvc)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Clean Architecture](https://docs.microsoft.com/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
