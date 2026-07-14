# AdventureWorks MVC Core - Clean Architecture

This project demonstrates a modern ASP.NET Core MVC application built with Clean Architecture principles on .NET 8.

## Architecture Overview

The solution follows Clean Architecture with clear separation of concerns across four layers:

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│              AdventureWorksMVCCore.Web                   │
│         (Controllers, Views, ViewModels)                 │
└─────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│                   Application Layer                      │
│           AdventureWorksMVCCore.Application              │
│        (Services, Interfaces, Business Logic)            │
└─────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                    │
│          AdventureWorksMVCCore.Infrastructure            │
│      (Data Access, Repositories, DbContext)              │
└─────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│                     Domain Layer                         │
│             AdventureWorksMVCCore.Domain                 │
│          (Entities, Interfaces, Core Logic)              │
└─────────────────────────────────────────────────────────┘
```

## Project Structure

### 1. AdventureWorksMVCCore.Domain
**Purpose**: Core business entities and repository interfaces

**Contents**:
- `Entities/` - Domain entities (Product, ProductCategory, ProductSubcategory)
- `Interfaces/` - Repository interfaces (IProductRepository, ICategoryRepository)

**Dependencies**: None (pure domain logic)

### 2. AdventureWorksMVCCore.Application
**Purpose**: Business logic and application services

**Contents**:
- `Services/` - Service implementations (ProductService, CategoryService)
- `Interfaces/` - Service interfaces (IProductService, ICategoryService)
- `DTOs/` - Data Transfer Objects (if needed)

**Dependencies**: 
- AdventureWorksMVCCore.Domain

### 3. AdventureWorksMVCCore.Infrastructure
**Purpose**: Data access and external service implementations

**Contents**:
- `Data/` - Entity Framework DbContext (CycleStoreContext)
- `Repositories/` - Repository implementations (ProductRepository, CategoryRepository)

**Dependencies**: 
- AdventureWorksMVCCore.Domain
- Microsoft.EntityFrameworkCore.SqlServer (8.0.0)
- Microsoft.EntityFrameworkCore.Tools (8.0.0)

### 4. AdventureWorksMVCCore.Web
**Purpose**: Presentation layer with MVC controllers and views

**Contents**:
- `Controllers/` - MVC controllers (HomeController, ProductsController, CartController)
- `Views/` - Razor views
- `ViewModels/` - View-specific models
- `Models/` - Helper models (CatalogImages, CatalogContent, CartStore)
- `wwwroot/` - Static files (CSS, JS, images)

**Dependencies**: 
- AdventureWorksMVCCore.Application
- AdventureWorksMVCCore.Infrastructure

## Key Benefits of Clean Architecture

1. **Separation of Concerns**: Each layer has a specific responsibility
2. **Testability**: Business logic can be tested independently of infrastructure
3. **Maintainability**: Changes in one layer don't affect others
4. **Flexibility**: Easy to swap implementations (e.g., change database provider)
5. **Dependency Rule**: Dependencies point inward (Web → Application → Infrastructure → Domain)

## Technology Stack

- **.NET 8** - Latest LTS version
- **ASP.NET Core MVC** - Web framework
- **Entity Framework Core 8.0** - ORM for data access
- **SQL Server** - Database
- **Dependency Injection** - Built-in DI container

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (local or remote)
- Visual Studio 2022 or VS Code

### Configuration

Update the connection string in `appsettings.json` or set the environment variable:

```bash
export ConnectionStrings__DefaultConnection="Server=your-server;Database=CYCLE_STORE;..."
```

### Build and Run

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the web application
cd AdventureWorksMVCCore.Web
dotnet run
```

## Database

The application uses the CYCLE_STORE database with the following schema:

- `Production.Product` - Product catalog
- `Production.ProductCategory` - Product categories
- `Production.ProductSubcategory` - Product subcategories

## Dependency Injection Setup

Services are registered in `Startup.cs`:

```csharp
// Infrastructure layer - Repositories
services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<ICategoryRepository, CategoryRepository>();

// Application layer - Services
services.AddScoped<IProductService, ProductService>();
services.AddScoped<ICategoryService, CategoryService>();

// DbContext
services.AddDbContext<CycleStoreContext>(options =>
    options.UseSqlServer(connectionString));
```

## Testing Strategy

The Clean Architecture enables comprehensive testing:

1. **Unit Tests**: Test Application layer services with mocked repositories
2. **Integration Tests**: Test Infrastructure layer with test database
3. **UI Tests**: Test Web layer controllers with mocked services

## Migration from Legacy ASP.NET MVC

This application has been migrated from legacy ASP.NET MVC to .NET 8 with Clean Architecture:

**Key Changes**:
- Separated concerns into distinct layers
- Introduced repository pattern for data access
- Moved business logic to Application layer
- Updated to Entity Framework Core 8.0
- Modernized dependency injection
- Improved testability and maintainability

## License

This is a demonstration project for educational purposes.
