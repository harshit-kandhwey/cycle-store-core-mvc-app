using System.Collections.Generic;
using AdventureWorksMVCCore.Domain.Entities;

namespace AdventureWorksMVCCore.Web.ViewModels
{
    /// <summary>
    /// View model for category page
    /// </summary>
    public class CategoryPageViewModel
    {
        public ProductCategory Category { get; set; }
        public List<ProductSubcategory> Subcategories { get; set; }
        public List<ProductCard> Products { get; set; }
    }
}
