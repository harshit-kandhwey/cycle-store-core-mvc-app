using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AdventureWorksMVCCore.Web.Models;
using AdventureWorksMVCCore.Web.Service.Interface;

namespace AdventureWorksMVCCore.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductService _productService;

        public CartController(IProductService productService)
        {
            _productService = productService;
        }

        private CartViewModel BuildCart()
        {
            var cart = CartStore.Get(HttpContext.Session);
            var map = _productService.SubcategoryMap();
            var vm = new CartViewModel();
            foreach (var kv in cart)
            {
                var p = _productService.GetProduct(kv.Key);
                if (p == null || !CatalogCuration.IsProductIncluded(p.ProductNumber)) continue;

                string image;
                if (p.ProductSubcategoryId.HasValue && map.TryGetValue(p.ProductSubcategoryId.Value, out var sub))
                    image = CatalogImages.For(sub.ProductCategory?.Name, sub.Name, p.ProductNumber, kv.Key);
                else
                    image = CatalogImages.For(null, null, p.ProductNumber, kv.Key);

                vm.Lines.Add(new CartLine { Product = p, Qty = kv.Value, Image = image });
            }
            vm.Count = vm.Lines.Sum(l => l.Qty);
            vm.Subtotal = vm.Lines.Sum(l => l.LineTotal);
            return vm;
        }

        // GET /Cart
        public IActionResult Index() => View(BuildCart());

        // POST /Cart/Add
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Add(int id, int qty = 1)
        {
            var p = _productService.GetProduct(id);
            if (p == null || !CatalogCuration.IsProductIncluded(p.ProductNumber))
            {
                return NotFound();
            }
            CartStore.Add(HttpContext.Session, id, qty);

            var count = CartStore.Count(HttpContext.Session);
            if (Request.Headers.TryGetValue("X-Requested-With", out var requestedWith) && requestedWith == "fetch")
            {
                return Json(new { count, name = p.Name });
            }
            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Update
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Update(int id, int qty)
        {
            CartStore.SetQty(HttpContext.Session, id, qty);
            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Remove
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Remove(int id)
        {
            CartStore.Remove(HttpContext.Session, id);
            return RedirectToAction(nameof(Index));
        }

        // GET /Cart/Checkout
        public IActionResult Checkout()
        {
            var cart = BuildCart();
            if (cart.Count == 0) return RedirectToAction(nameof(Index));
            ViewBag.Cart = cart;
            return View(new CheckoutModel());
        }

        // POST /Cart/PlaceOrder
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult PlaceOrder(CheckoutModel model)
        {
            var cart = BuildCart();
            if (cart.Count == 0) return RedirectToAction(nameof(Index));
            if (!ModelState.IsValid)
            {
                ViewBag.Cart = cart;
                return View("Checkout", model);
            }

            var orderNo = "CS-" + DateTime.Now.ToString("yyMMdd") + "-" +
                          Guid.NewGuid().ToString("N").Substring(0, 5).ToUpperInvariant();
            CartStore.Clear(HttpContext.Session);

            TempData["OrderNo"] = orderNo;
            TempData["OrderEmail"] = model.Email;
            TempData["OrderTotal"] = cart.Subtotal.ToString("N2");
            TempData["OrderItems"] = cart.Count;
            return RedirectToAction(nameof(Confirmation));
        }

        // GET /Cart/Confirmation
        public IActionResult Confirmation()
        {
            if (TempData["OrderNo"] == null) return RedirectToAction("Index", "Home");
            return View();
        }
    }
}
