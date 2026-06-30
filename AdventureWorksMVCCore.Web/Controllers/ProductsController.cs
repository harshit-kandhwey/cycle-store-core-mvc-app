using Microsoft.AspNetCore.Mvc;
using AdventureWorksMVCCore.Web.Models;
using AdventureWorksMVCCore.Web.Service.Interface;

namespace AdventureWorksMVCCore.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET /Products/Subcategory/{id} — list all products in a subcategory.
        public IActionResult Subcategory(int id)
        {
            var subcategory = _productService.GetSubcategory(id);
            if (subcategory == null)
            {
                return NotFound();
            }

            var model = new SubcategoryProductsViewModel
            {
                Subcategory = subcategory,
                Products = _productService.GetProductsBySubcategory(id)
            };
            return View(model);
        }

        // GET /Products/Details/{id} — single product detail.
        public IActionResult Details(int id)
        {
            var product = _productService.GetProduct(id);
            if (product == null)
            {
                return NotFound();
            }

            // Resolve the product's category so the view can pick a category-appropriate image.
            if (product.ProductSubcategoryId.HasValue)
            {
                var sub = _productService.GetSubcategory(product.ProductSubcategoryId.Value);
                ViewBag.Category = sub?.ProductCategory?.Name;
                ViewBag.Subcategory = sub?.Name;
            }
            return View(product);
        }
    }
}
