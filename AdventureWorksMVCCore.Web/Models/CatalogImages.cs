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
                var path = ProductImagePath(Slug(productNumber));
                if (path != null) return path;
            }
            return For(category, subcategory, index);
        }

        // Accepted on-disk formats for a product photo, in preference order.
        private static readonly string[] ImageExts = { ".jpg", ".jpeg", ".png", ".webp", ".avif" };

        // Cache disk lookups so we don't stat the file on every render. The cached value
        // is the resolved web path ("~/Images/catalog/product/{slug}{ext}"), or null when
        // no file exists in any accepted format.
        private static readonly ConcurrentDictionary<string, string> _productImageCache =
            new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static string ProductImagePath(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return null;
            return _productImageCache.GetOrAdd(slug, s =>
            {
                var dir = Path.Combine(AppContext.BaseDirectory, "wwwroot", "Images", "catalog", "product");
                foreach (var ext in ImageExts)
                {
                    if (File.Exists(Path.Combine(dir, s + ext)))
                        return "~/Images/catalog/product/" + s + ext;
                }
                return null;
            });
        }

        private static bool ProductImageExists(string slug) => ProductImagePath(slug) != null;

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

        /// <summary>
        /// Ordered list of images for a product gallery: the per-product photo and any
        /// numbered angles (…/product/{slug}-2.jpg …-3.jpg) that exist on disk, then the
        /// subcategory photo, then category photos — deduped and capped. Always non-empty.
        /// </summary>
        public static List<string> Gallery(string category, string subcategory, string productNumber, int index)
        {
            var list = new List<string>();
            void Add(string p) { if (!string.IsNullOrEmpty(p) && !list.Contains(p)) list.Add(p); }

            var pslug = Slug(productNumber);
            if (CatalogCuration.IsProductIncluded(productNumber))
            {
                var main = ProductImagePath(pslug);
                if (main != null)
                {
                    Add(main);
                    for (var n = 2; n <= 4; n++)
                    {
                        var angle = ProductImagePath(pslug + "-" + n);
                        if (angle != null) Add(angle);
                    }
                }
            }

            var sslug = Slug(subcategory);
            if (SubSlugs.Contains(sslug))
                Add("~/Images/catalog/sub/" + sslug + ".jpg");

            var key = CategorySets.Keys.FirstOrDefault(k => (category ?? "").IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
            var set = key != null ? CategorySets[key] : CategorySets["Bikes"];
            foreach (var nm in set.Take(3))
                Add("~/Images/catalog/" + nm + ".jpg");

            return list.Take(5).ToList();
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
