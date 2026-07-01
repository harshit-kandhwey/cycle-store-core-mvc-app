using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventureWorksMVCCore.Web.Models
{
    /// <summary>
    /// Maps a product to a bundled image under wwwroot/Images/catalog.
    /// Prefers a subcategory-specific photo (…/catalog/sub/{slug}.jpg); falls back to a
    /// category photo when a subcategory image is not available.
    /// </summary>
    public static class CatalogImages
    {
        // Subcategories that have a dedicated photo under wwwroot/Images/catalog/sub/.
        private static readonly HashSet<string> SubSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "mountain-bikes","road-bikes","touring-bikes",
            "bib-shorts","gloves","jerseys","shorts","socks","tights","vests",
            "bike-racks","bike-stands","bottles-and-cages","fenders","helmets","hydration-packs",
            "lights","locks","panniers","pumps","tires-and-tubes",
            "handlebars","bottom-brackets","brakes","chains","cranksets","derailleurs","forks",
            "headsets","mountain-frames","pedals","road-frames","saddles","touring-frames","wheels"
            // note: "caps" and "cleaners" intentionally fall back to their category image
        };

        private static readonly Dictionary<string, string[]> CategorySets =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Bikes"] = new[] { "bikes-1", "bikes-2", "bikes-3", "bikes-4", "bikes-5", "bikes-6" },
            ["Components"] = new[] { "components-1", "components-2", "components-3" },
            ["Clothing"] = new[] { "clothing-1", "clothing-2", "clothing-3" },
            ["Accessories"] = new[] { "accessories-1", "accessories-2", "accessories-3" },
        };

        private static readonly Dictionary<string, string> CategoryBanner =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Bikes"] = "sub/road-bikes",
            ["Components"] = "sub/chains",
            ["Clothing"] = "sub/jerseys",
            ["Accessories"] = "sub/helmets",
        };

        public static string Slug(string s)
        {
            s = (s ?? "").ToLowerInvariant().Replace("&", "and");
            return Regex.Replace(s, "[^a-z0-9]+", "-").Trim('-');
        }

        /// <summary>
        /// Image path for a product, preferring a per-product photo
        /// (…/catalog/product/{product-number}.jpg) when one is available,
        /// then a subcategory photo, then a category photo.
        /// </summary>
        public static string For(string category, string subcategory, string productNumber, int index)
        {
            if (CatalogCuration.IsProductIncluded(productNumber))
            {
                var slug = Slug(productNumber);
                if (ProductImageExists(slug))
                    return "~/Images/catalog/product/" + slug + ".jpg";
            }
            return For(category, subcategory, index);
        }

        // Cache disk lookups so we don't stat the file on every render.
        private static readonly ConcurrentDictionary<string, bool> _productImageCache =
            new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        private static bool ProductImageExists(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return false;
            return _productImageCache.GetOrAdd(slug, s =>
            {
                var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "Images", "catalog", "product", s + ".jpg");
                return File.Exists(path);
            });
        }

        /// <summary>Image path for a product, preferring a subcategory-specific photo.</summary>
        public static string For(string category, string subcategory, int index)
        {
            var slug = Slug(subcategory);
            if (SubSlugs.Contains(slug))
                return "~/Images/catalog/sub/" + slug + ".jpg";

            var key = CategorySets.Keys.FirstOrDefault(k => (category ?? "").IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
            var set = key != null ? CategorySets[key] : CategorySets["Bikes"];
            var name = set[((index % set.Length) + set.Length) % set.Length];
            return "~/Images/catalog/" + name + ".jpg";
        }

        /// <summary>Banner image for a category card on the home page.</summary>
        public static string Banner(string category)
        {
            var key = CategoryBanner.Keys.FirstOrDefault(k => (category ?? "").IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
            var name = key != null ? CategoryBanner[key] : "hero";
            return "~/Images/catalog/" + name + ".jpg";
        }
    }
}
