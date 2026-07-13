using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AdventureWorksMVCCore.Web.Models;
using AdventureWorksMVCCore.Web.Service.Interface;

namespace AdventureWorksMVCCore.Web.Controllers
{
        private readonly CatalogImages _catalogImages;
        private readonly CartStore _cartStore;
        public CartController(IProductService productService, CatalogImages catalogImages, CartStore cartStore)
            _catalogImages = catalogImages;
            _cartStore = cartStore;
            var cart = _cartStore.Get(HttpContext.Session);
        {
            var cart = CartStore.Get(HttpContext.Session);
            var map = _productService.SubcategoryMap();
            var vm = new CartViewModel();
            foreach (var kv in cart)
            {
                var p = _productService.GetProduct(kv.Key);
                if (p == null || !CatalogCuration.IsProductIncluded(p.ProductNumber)) continue;
                    image = _catalogImages.For(sub.ProductCategory?.Name, sub.Name, p.ProductNumber, kv.Key);
                    image = _catalogImages.For(null, null, p.ProductNumber, kv.Key);
            vm.Count = vm.Lines.Sum(l => l.Qty);
            vm.Subtotal = vm.Lines.Sum(l => l.LineTotal);
            return vm;
        }

        // GET /Cart
        [HttpGet]
        public IActionResult Index() => View(BuildCart());

        // POST /Cart/Add
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Add(int id, int qty = 1)
        {
            var p = _productService.GetProduct(id);
            if (p == null || !CatalogCuration.IsProductIncluded(p.ProductNumber))
            {
            _cartStore.Add(HttpContext.Session, id, qty);
            var count = _cartStore.Count(HttpContext.Session);
            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Update
        [HttpPost, ValidateAntiForgeryToken]
            _cartStore.SetQty(HttpContext.Session, id, qty);
        [HttpPost, ValidateAntiForgeryToken]
            _cartStore.Remove(HttpContext.Session, id);
        [HttpGet]
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
            _cartStore.Clear(HttpContext.Session);
            TempData["OrderTotal"] = cart.Subtotal.ToString("N2");
            TempData["OrderItems"] = cart.Count;
            return RedirectToAction(nameof(Confirmation));
        }

        // GET /Cart/Confirmation
        [HttpGet]
        public IActionResult Confirmation()
        {
            if (TempData["OrderNo"] == null) return RedirectToAction("Index", "Home");
            return View();
        }
    }
}
