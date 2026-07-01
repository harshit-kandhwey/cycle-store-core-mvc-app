using System;
using System.Collections.Generic;

namespace AdventureWorksMVCCore.Web.Models
{
    /// <summary>
    /// Non-destructive, app-side trim of the live catalog for the UI-revamp demo.
    /// The database is untouched; these allow-lists just decide what the storefront
    /// shows so every visible product can have a hand-picked photo.
    /// Each product number here also has a dedicated image under
    /// wwwroot/Images/catalog/product/{product-number}.jpg.
    /// </summary>
    public static class CatalogCuration
    {
        // Subcategories shown on the home page and browsable.
        public static readonly HashSet<string> Subcategories =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Mountain Bikes", "Road Bikes", "Cranksets", "Jerseys", "Gloves", "Helmets"
        };

        // Products shown within those subcategories (one distinct model per entry).
        public static readonly HashSet<string> Products =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BK-M82B-38", "BK-M68S-38", "BK-M18B-40",   // Mountain Bikes
            "BK-R93R-44", "BK-R68R-44", "BK-R19B-44",   // Road Bikes
            "CS-9183", "CS-6583", "CS-4759",            // Cranksets
            "LJ-0192-L", "SJ-0194-L",                   // Jerseys
            "GL-F110-L", "GL-H102-L",                   // Gloves
            "HL-U509", "HL-U509-B", "HL-U509-R"         // Helmets
        };

        public static bool IsSubcategoryIncluded(string name)
            => name != null && Subcategories.Contains(name.Trim());

        public static bool IsProductIncluded(string productNumber)
            => productNumber != null && Products.Contains(productNumber.Trim());
    }
}
