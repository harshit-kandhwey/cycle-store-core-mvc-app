using System;
using System.Linq;
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

        // GET /Products/Subcategory/{id}?sort=&color=
        public IActionResult Subcategory(int id, string sort = null, string color = null)
        {
            var subcategory = _productService.GetSubcategory(id);
            if (subcategory == null || !CatalogCuration.IsSubcategoryIncluded(subcategory.Name))
            {
                return NotFound();
            }

            var products = _productService.GetProductsBySubcategory(id)
                .Where(p => CatalogCuration.IsProductIncluded(p.ProductNumber))
                .ToList();

            // Colours available for the filter UI (computed before filtering).
            var colors = products
                .Where(p => !string.IsNullOrWhiteSpace(p.Color))
                .Select(p => p.Color).Distinct()
                .OrderBy(c => c).ToList();

            if (!string.IsNullOrWhiteSpace(color))
            {
                products = products
                    .Where(p => string.Equals(p.Color, color, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            products = (sort switch
            {
                "price-asc" => products.OrderBy(p => p.ListPrice),
                "price-desc" => products.OrderByDescending(p => p.ListPrice),
                "name-desc" => products.OrderByDescending(p => p.Name),
                _ => products.OrderBy(p => p.Name)
            }).ToList();

            ViewBag.Sort = sort;
            ViewBag.Color = color;
            ViewBag.Colors = colors;

            var model = new SubcategoryProductsViewModel
            {
                Subcategory = subcategory,
                Products = products
            };
            return View(model);
        }

        // GET /Products/Details/{id}
        public IActionResult Details(int id)
        {
            var product = _productService.GetProduct(id);
            if (product == null)
            {
                return NotFound();
            }

            if (product.ProductSubcategoryId.HasValue)
            {
                var sub = _productService.GetSubcategory(product.ProductSubcategoryId.Value);
                ViewBag.Category = sub?.ProductCategory?.Name;
                ViewBag.Subcategory = sub?.Name;
            }
            return View(product);
        }

        // GET /Products/Search?q=
        public IActionResult Search(string q)
        {
            var results = _productService.Search(q)
                .Where(p => CatalogCuration.IsProductIncluded(p.ProductNumber))
                .ToList();
            var map = _productService.SubcategoryMap();

            var cards = results.Select((p, i) =>
            {
                string image;
                if (p.ProductSubcategoryId.HasValue && map.TryGetValue(p.ProductSubcategoryId.Value, out var sub))
                {
                    image = CatalogImages.For(sub.ProductCategory?.Name, sub.Name, p.ProductNumber, i);
                }
                else
                {
                    image = CatalogImages.For(null, null, p.ProductNumber, i);
                }
                return new ProductCard { Product = p, Image = image };
            }).ToList();

            ViewBag.Query = q;
            return View(cards);
        }
    }
}
