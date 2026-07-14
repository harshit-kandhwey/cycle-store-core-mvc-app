using System.Collections.Generic;
using AdventureWorksMVCCore.Domain.Entities;

namespace AdventureWorksMVCCore.Web.ViewModels
{
    /// <summary>
    /// View model for subcategory products page
    /// </summary>
    public class SubcategoryProductsViewModel
    {
        public ProductSubcategory Subcategory { get; set; }
        public List<Product> Products { get; set; }
    }
}
