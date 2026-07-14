using System;
using System.Collections.Generic;

namespace AdventureWorksMVCCore.Domain.Entities
{
    /// <summary>
    /// Domain entity representing a product category
    /// </summary>
    public class ProductCategory
    {
        public ProductCategory()
        {
            ProductSubcategory = new HashSet<ProductSubcategory>();
        }

        public int ProductCategoryId { get; set; }
        public string Name { get; set; }
        public Guid Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }

        // Navigation property
        public virtual ICollection<ProductSubcategory> ProductSubcategory { get; set; }
    }
}
