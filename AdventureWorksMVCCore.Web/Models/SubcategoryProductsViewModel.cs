using System.Collections.Generic;

namespace AdventureWorksMVCCore.Web.Models
{
    public class SubcategoryProductsViewModel
    {
        public ProductSubcategory Subcategory { get; set; }
        public List<Product> Products { get; set; }
    }
}
