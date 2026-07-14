using AdventureWorksMVCCore.Domain.Entities;

namespace AdventureWorksMVCCore.Web.ViewModels
{
    /// <summary>
    /// View model for product card display
    /// </summary>
    public class ProductCard
    {
        public Product Product { get; set; }
        public string Image { get; set; }
    }
}
