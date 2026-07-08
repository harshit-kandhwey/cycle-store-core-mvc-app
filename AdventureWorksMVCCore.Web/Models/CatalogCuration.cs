using System;
using System.Collections.Generic;

namespace AdventureWorksMVCCore.Web.Models
{
    /// <summary>
    /// App-side curation of the storefront catalog for the UI-revamp demo.
    /// These allow-lists decide which subcategories and products the storefront shows.
    /// The catalog was pivoted to a hand-built set of real cycling products (rows
    /// ProductID 1000-1072 in the CYCLE_STORE database); each product number here has a
    /// dedicated photo under wwwroot/Images/catalog/product/{product-number}.{jpg|avif|...}.
    /// </summary>
    public static class CatalogCuration
    {
        // Subcategories shown on the home page and browsable.
        public static readonly HashSet<string> Subcategories =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Bikes
            "Mountain Bikes",
            "Road Bikes",
            "Touring Bikes",

            // Components
            "Saddles",
            "Chains",
            "Derailleurs",
            "Brakes",
            "Handlebars",
            "Wheels",
            "Bells",
            "Cranksets",
            "Pedals",

            // Clothing
            "Helmets",
            "Gloves",
            "Jerseys",
            "Shorts",
            "Caps",
            "Vests",
            "Tights",
            "Backpacks",
            "Sunglasses",
            "Protectors",

            // Accessories
            "Lights",
            "Locks",
            "Pumps",
            "Bottles and Cages",
            "Fenders",
            "Panniers",
            "Bike Racks",
            "Bike Stands",
            "Cleaners",
            "Tools",
        };

        // Products shown within those subcategories (one entry per product number).
        public static readonly HashSet<string> Products =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BX-MTB-01", "BX-MTB-02", "BX-MTB-03", "BX-MTB-04", "BX-MTB-05", "BX-MTB-06",   // Mountain Bikes
            "BX-RB-01", "BX-RB-02",   // Road Bikes
            "BX-TB-01", "BX-TB-02", "BX-TB-03", "BX-TB-04", "BX-TB-05",   // Touring Bikes
            "PX-SDL-01", "PX-SDL-02", "PX-SDL-03", "PX-SDL-04", "PX-SDL-05",   // Saddles
            "PX-CHN-01", "PX-CHN-02",   // Chains
            "PX-DER-01",   // Derailleurs
            "PX-BRK-01", "PX-BRK-02", "PX-BRK-03", "PX-BRK-04", "PX-BRK-05",   // Brakes
            "PX-HBR-01", "PX-HBR-02", "PX-HBR-03",   // Handlebars
            "PX-WHL-01", "PX-WHL-02",   // Wheels
            "PX-BEL-01",   // Bells
            "PX-CAS-01", "PX-CAS-02",   // Cranksets
            "PX-PED-01", "PX-PED-02", "PX-PED-03", "PX-PED-04",   // Pedals
            "AX-HLM-01", "AX-HLM-02", "AX-HLM-03", "AX-HLM-04", "AX-HLM-05", "AX-HLM-06", "AX-HLM-07", "AX-HLM-08",   // Helmets
            "CX-GLV-01", "CX-GLV-02", "CX-GLV-03", "CX-GLV-04", "CX-GLV-05", "CX-GLV-06", "CX-GLV-07",   // Gloves
            "CX-JRS-01", "CX-JRS-02", "CX-JRS-03", "CX-JRS-04",   // Jerseys
            "CX-SRT-01",   // Shorts
            "CX-CAP-01",   // Caps
            "CX-VST-01",   // Vests
            "CX-TGT-01", "CX-TGT-02",   // Tights
            "CX-BAG-01",   // Backpacks
            "CX-SUN-01", "CX-SUN-02", "CX-SUN-03", "CX-SUN-04",   // Sunglasses
            "CX-PRO-01",   // Protectors
            "AX-LGT-01", "AX-LGT-02", "AX-LGT-03",   // Lights
            "AX-LCK-01",   // Locks
            "AX-PMP-01",   // Pumps
            "AX-BTL-01", "AX-BTL-02",   // Bottles and Cages
            "AX-FND-01", "AX-FND-02",   // Fenders
            "AX-PAN-01", "AX-PAN-02",   // Panniers
            "AX-RCK-01",   // Bike Racks
            "AX-STD-01", "AX-STD-02",   // Bike Stands
            "AX-CLN-01", "AX-CLN-02", "AX-CLN-03",   // Cleaners
            "AX-TOL-01", "AX-TOL-02",   // Tools
        };

        public static bool IsSubcategoryIncluded(string name)
            => name != null && Subcategories.Contains(name.Trim());

        public static bool IsProductIncluded(string productNumber)
            => productNumber != null && Products.Contains(productNumber.Trim());
    }
}
