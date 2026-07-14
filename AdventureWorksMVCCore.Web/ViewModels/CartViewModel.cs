using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AdventureWorksMVCCore.Domain.Entities;

namespace AdventureWorksMVCCore.Web.ViewModels
{
    /// <summary>
    /// Cart line item
    /// </summary>
    public class CartLine
    {
        public Product Product { get; set; }
        public int Qty { get; set; }
        public string Image { get; set; }
        public decimal LineTotal => Product == null ? 0 : Product.ListPrice * Qty;
    }

    /// <summary>
    /// Shopping cart view model
    /// </summary>
    public class CartViewModel
    {
        public List<CartLine> Lines { get; set; } = new List<CartLine>();
        public int Count { get; set; }
        public decimal Subtotal { get; set; }
    }

    /// <summary>
    /// Checkout form model
    /// </summary>
    public class CheckoutModel
    {
        [Required, StringLength(80)]
        public string Name { get; set; }

        [Required, EmailAddress, StringLength(120)]
        public string Email { get; set; }

        [Required, StringLength(160)]
        public string Address { get; set; }

        [Required, StringLength(60)]
        public string City { get; set; }

        [Required, StringLength(16)]
        [Display(Name = "Postcode")]
        public string Zip { get; set; }
    }
}
