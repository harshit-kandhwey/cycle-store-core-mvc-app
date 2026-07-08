using System.Collections.Generic;

namespace AdventureWorksMVCCore.Web.Models
{
    /// <summary>Data for a category landing page: the category, its (curated)
    /// subcategories, and the products shown across them.</summary>
    public class CategoryPageViewModel
    {
        public ProductCategory Category { get; set; }
        public List<ProductSubcategory> Subcategories { get; set; }
        public List<ProductCard> Products { get; set; }
    }
}
