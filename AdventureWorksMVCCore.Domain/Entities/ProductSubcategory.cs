using System;

namespace AdventureWorksMVCCore.Domain.Entities
{
    /// <summary>
    /// Domain entity representing a product subcategory
    /// </summary>
    public class ProductSubcategory
    {
        public int ProductSubcategoryId { get; set; }
        public int ProductCategoryId { get; set; }
        public string Name { get; set; }
        public Guid Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }

        // Navigation property
        public virtual ProductCategory ProductCategory { get; set; }
    }
}
