using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventureWorksMVCCore.Web.Models
{
    /// <summary>
    /// Maps a product to a bundled, category-appropriate image under
    /// wwwroot/Images/catalog. Images rotate within a category for visual variety.
    /// </summary>
    public static class CatalogImages
    {
        private static readonly Dictionary<string, string[]> Sets =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Bikes"] = new[] { "bikes-1", "bikes-2", "bikes-3", "bikes-4", "bikes-5", "bikes-6" },
            ["Components"] = new[] { "components-1", "components-2", "components-3" },
            ["Clothing"] = new[] { "clothing-1", "clothing-2", "clothing-3" },
            ["Accessories"] = new[] { "accessories-1", "accessories-2", "accessories-3" },
        };

        /// <summary>Returns an app-relative (~/) image path for the given category and item index.</summary>
        public static string For(string category, int index)
        {
            category ??= "";
            var key = Sets.Keys.FirstOrDefault(k => category.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
            var set = key != null ? Sets[key] : Sets["Bikes"];
            var name = set[((index % set.Length) + set.Length) % set.Length];
            return "~/Images/catalog/" + name + ".jpg";
        }
    }
}
