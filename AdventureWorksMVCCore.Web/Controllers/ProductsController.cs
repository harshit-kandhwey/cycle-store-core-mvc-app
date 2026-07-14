using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using AdventureWorksMVCCore.Application.Interfaces;
using AdventureWorksMVCCore.Domain.Entities;
using AdventureWorksMVCCore.Web.Models;
using AdventureWorksMVCCore.Web.ViewModels;

namespace AdventureWorksMVCCore.Web.Controllers
{
    /// <summary>
    /// Products controller for catalog browsing and search
    /// </summary>
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductsController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        // GET /Products/Category/{id}   (id = category name, e.g. "Bikes")
        [HttpGet]
        public IActionResult Category(string id)
        {
            var category = _categoryService.GetCategoriesWithSubCategory()
                .FirstOrDefault(c => string.Equals(c.Name, id, StringComparison.OrdinalIgnoreCase));
            if (category == null)
            {
                return NotFound();
            }

            var subs = category.ProductSubcategory
                .Where(s => CatalogCuration.IsSubcategoryIncluded(s.Name))
                .OrderBy(s => s.Name).ToList();
            if (subs.Count == 0)
            {
                return NotFound();
            }

            var cards = new List<ProductCard>();
            var i = 0;
            foreach (var s in subs)
            {
                var prods = _productService.GetProductsBySubcategory(s.ProductSubcategoryId)
                    .Where(p => CatalogCuration.IsProductIncluded(p.ProductNumber));
                foreach (var p in prods)
                {
                    cards.Add(new ProductCard
                    {
                        Product = p,
                        Image = CatalogImages.For(category.Name, s.Name, p.ProductNumber, i)
                    });
                    i++;
                }
            }

            return View(new CategoryPageViewModel
            {
                Category = category,
                Subcategories = subs,
                Products = cards
            });
        }

        // GET /Products/Subcategory/{id}?sort=&color=&color=&min=&max=&inStock=
        [HttpGet]
        public IActionResult Subcategory(int id, string sort = null, string[] color = null,
            decimal? min = null, decimal? max = null, bool inStock = false, string[] brand = null)
        {
            var subcategory = _productService.GetSubcategory(id);
            if (subcategory == null || !CatalogCuration.IsSubcategoryIncluded(subcategory.Name))
            {
                return NotFound();
            }

            var all = _productService.GetProductsBySubcategory(id)
                .Where(p => CatalogCuration.IsProductIncluded(p.ProductNumber))
                .ToList();

            // Facets computed from the full (pre-filter) set.
            var colors = all
                .Where(p => !string.IsNullOrWhiteSpace(p.Color))
                .Select(p => p.Color).Distinct()
                .OrderBy(c => c).ToList();
            var priceFloor = all.Any() ? Math.Floor(all.Min(p => p.ListPrice)) : 0m;
            var priceCeil = all.Any() ? Math.Ceiling(all.Max(p => p.ListPrice)) : 0m;
            var brands = all
                .Select(p => CatalogContent.Brand(p.Name))
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(b => b).ToList();

            var selColors = (color ?? Array.Empty<string>())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var selBrands = (brand ?? Array.Empty<string>())
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            IEnumerable<Product> query = all;
            if (selColors.Count > 0)
            {
                query = query.Where(p => selColors.Contains(p.Color, StringComparer.OrdinalIgnoreCase));
            }
            if (selBrands.Count > 0)
            {
                query = query.Where(p => selBrands.Contains(CatalogContent.Brand(p.Name), StringComparer.OrdinalIgnoreCase));
            }
            if (min.HasValue)
            {
                query = query.Where(p => p.ListPrice >= min.Value);
            }
            if (max.HasValue)
            {
                query = query.Where(p => p.ListPrice <= max.Value);
            }
            if (inStock)
            {
                query = query.Where(p => p.SellEndDate == null && p.DiscontinuedDate == null);
            }

            var products = (sort switch
            {
                "price-asc" => query.OrderBy(p => p.ListPrice),
                "price-desc" => query.OrderByDescending(p => p.ListPrice),
                "name-desc" => query.OrderByDescending(p => p.Name),
                _ => query.OrderBy(p => p.Name)
            }).ToList();

            ViewBag.Sort = sort;
            ViewBag.Colors = colors;
            ViewBag.SelColors = selColors;
            ViewBag.Brands = brands;
            ViewBag.SelBrands = selBrands;
            ViewBag.Min = min;
            ViewBag.Max = max;
            ViewBag.PriceFloor = priceFloor;
            ViewBag.PriceCeil = priceCeil;
            ViewBag.InStock = inStock;
            ViewBag.TotalCount = all.Count;

            var model = new SubcategoryProductsViewModel
            {
                Subcategory = subcategory,
                Products = products
            };
            return View(model);
        }

        // GET /Products/Details/{id}
        [HttpGet]
        public IActionResult Details(int id)
        {
            var product = _productService.GetProduct(id);
            if (product == null)
            {
                return NotFound();
            }

            string cat = null, sub = null;
            if (product.ProductSubcategoryId.HasValue)
            {
                var s = _productService.GetSubcategory(product.ProductSubcategoryId.Value);
                cat = s?.ProductCategory?.Name;
                sub = s?.Name;
                ViewBag.Category = cat;
                ViewBag.Subcategory = sub;

                var related = _productService.GetProductsBySubcategory(product.ProductSubcategoryId.Value)
                    .Where(p => CatalogCuration.IsProductIncluded(p.ProductNumber) && p.ProductId != product.ProductId)
                    .Take(4)
                    .Select((p, i) => new ProductCard
                    {
                        Product = p,
                        Image = CatalogImages.For(cat, sub, p.ProductNumber, i)
                    }).ToList();
                ViewBag.Related = related;
            }

            ViewBag.Gallery = CatalogImages.Gallery(cat, sub, product.ProductNumber, product.ProductId);
            return View(product);
        }

        // GET /Products/QuickView/{id}  — HTML fragment for the quick-view modal
        [HttpGet]
        public IActionResult QuickView(int id)
        {
            var product = _productService.GetProduct(id);
            if (product == null)
            {
                return NotFound();
            }

            string cat = null, sub = null;
            if (product.ProductSubcategoryId.HasValue)
            {
                var s = _productService.GetSubcategory(product.ProductSubcategoryId.Value);
                cat = s?.ProductCategory?.Name;
                sub = s?.Name;
            }
            ViewBag.Category = cat;
            ViewBag.Subcategory = sub;
            ViewBag.Gallery = CatalogImages.Gallery(cat, sub, product.ProductNumber, product.ProductId);
            return PartialView("_QuickView", product);
        }

        // GET /Products/Search?q=
        [HttpGet]
        public IActionResult Search(string q)
        {
            // Normalise and bound the query so an oversized or blank term can't
            // drive an expensive Contains() scan against the product table.
            q = (q ?? string.Empty).Trim();
            if (q.Length > 100)
            {
                q = q.Substring(0, 100);
            }

            var results = string.IsNullOrWhiteSpace(q)
                ? new List<Product>()
                : _productService.Search(q)
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
