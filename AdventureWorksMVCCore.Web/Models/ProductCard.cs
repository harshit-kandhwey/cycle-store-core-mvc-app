namespace AdventureWorksMVCCore.Web.Models
{
    // A product plus its resolved (app-relative) image path, for grid rendering.
    public class ProductCard
    {
        public Product Product { get; set; }
        public string Image { get; set; }
    }
}
