using System;
using System.Collections.Generic;

namespace AdventureWorksMVCCore.Web.Models
{
    /// <summary>
    /// App-side, database-free enrichment for the storefront: brand detection, generated
    /// product descriptions, deterministic ratings/reviews, and a "was" (compare-at) price
    /// so the current ListPrice can be shown as a discounted sale price. Everything is a
    /// pure function of the product so it stays stable across requests without extra storage.
    /// </summary>
    public static class CatalogContent
    {
        // ---- Brand detection (from the product name) --------------------------------
        // Checked in order; first token found wins.
        private static readonly (string Token, string Brand)[] BrandTokens =
        {
            ("Van Rysel", "Van Rysel"), ("Rockrider", "Rockrider"), ("Triban", "Triban"),
            ("Riverside", "Riverside"), ("BTwin", "B'Twin"), ("B'Twin", "B'Twin"),
            ("Shimano", "Shimano"), ("Specialized", "Specialized"), ("Scott", "Scott"),
            ("Sidebike", "Sidebike"), ("Giant", "Giant"), ("Lake", "Lake"),
        };

        public static string Brand(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            foreach (var (token, brand) in BrandTokens)
                if (name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    return brand;
            return null;
        }

        // Stable, non-negative hash. (string.GetHashCode is randomised per process, so it
        // can't be used for values that must be identical across requests/servers.)
        private static int Seed(string s)
        {
            unchecked
            {
                int h = 17;
                foreach (var c in s ?? string.Empty) h = h * 31 + c;
                return h & 0x7fffffff;
            }
        }

        // ---- Ratings & reviews ------------------------------------------------------
        public static double Rating(string productNumber)
            => Math.Round(3.6 + (Seed(productNumber) % 15) * 0.1, 1); // 3.6 .. 5.0

        public static int ReviewCount(string productNumber)
            => 6 + Seed(productNumber + "n") % 240;

        private static readonly string[] Reviewers =
        {
            "Alex M.", "Priya S.", "Jordan T.", "Sam K.", "Chris D.", "Neha R.",
            "Marco V.", "Emily W.", "Ravi P.", "Dana L.", "Tom H.", "Aisha B.",
        };
        private static readonly string[] Praise =
        {
            "Exactly as described and great value for the money.",
            "Really solid quality — it has held up well so far.",
            "Comfortable, well made, and does the job perfectly.",
            "Arrived quickly and works exactly as expected.",
            "A great upgrade over the one I had before.",
            "Looks even better in person than in the photos.",
        };

        public static List<(string Author, int Stars, string Text)> Reviews(string productNumber)
        {
            var n = 2 + Seed(productNumber) % 2; // 2 or 3
            var list = new List<(string, int, string)>();
            for (int i = 0; i < n; i++)
            {
                var s = Seed(productNumber + "r" + i);
                list.Add((Reviewers[s % Reviewers.Length],
                          4 + (s / 7) % 2,               // 4 or 5 stars
                          Praise[(s / 13) % Praise.Length]));
            }
            return list;
        }

        // ---- Sale / compare-at price ------------------------------------------------
        // ListPrice is treated as the current (sale) price; this returns a higher "was"
        // price to strike through, deterministically per product.
        public static decimal CompareAt(string productNumber, decimal listPrice)
        {
            var markup = 1.20m + (Seed(productNumber + "p") % 31) / 100m; // 1.20 .. 1.50
            return Math.Ceiling(listPrice * markup) - 0.01m;             // retail-looking .99
        }

        public static int DiscountPercent(string productNumber, decimal listPrice)
        {
            var was = CompareAt(productNumber, listPrice);
            return was <= 0 ? 0 : (int)Math.Round((1 - listPrice / was) * 100m);
        }

        // ---- Descriptions -----------------------------------------------------------
        private static readonly Dictionary<string, string> SubBlurb =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Mountain Bikes"] = "built to take on trails, climbs and rough descents with confident control",
                ["Road Bikes"] = "tuned for speed and efficiency on tarmac with a light, responsive frame",
                ["Touring Bikes"] = "an easy-going all-rounder for commutes, cruising and weekend rides",
                ["Handlebars"] = "a precise cockpit upgrade for better control and comfort",
                ["Bottom Brackets"] = "a smooth, sealed spindle that keeps your drivetrain turning freely",
                ["Brakes"] = "reliable stopping power for confident control in any conditions",
                ["Chains"] = "a hard-wearing chain for smooth, quiet shifting",
                ["Cranksets"] = "a stiff, efficient drivetrain component for strong power transfer",
                ["Derailleurs"] = "crisp, dependable shifting across the full range of gears",
                ["Forks"] = "controlled, responsive front-end performance over rough ground",
                ["Pedals"] = "a grippy, durable platform for everyday and off-road riding",
                ["Saddles"] = "all-day comfort with support in the places that matter",
                ["Wheels"] = "a strong, true and reliable roll for mile after mile",
                ["Bells"] = "a crisp, clear ring to let others know you're there",
                ["Caps"] = "lightweight headwear that wicks sweat and blocks the sun",
                ["Gloves"] = "adding grip, comfort and protection on every ride",
                ["Jerseys"] = "breathable, quick-drying kit that keeps you comfortable mile after mile",
                ["Shorts"] = "supportive, chafe-free comfort for longer days in the saddle",
                ["Socks"] = "cushioned, breathable and made to go the distance",
                ["Tights"] = "warm, stretchy coverage for cooler-weather riding",
                ["Vests"] = "packable wind and weather protection that layers easily",
                ["Helmets"] = "protective, well-ventilated headgear for safer, cooler rides",
                ["Sunglasses"] = "clear, protective vision with a secure, comfortable fit",
                ["Protectors"] = "lightweight impact protection that stays put while you ride",
                ["Backpacks"] = "a comfortable, practical carry for the commute and beyond",
                ["Shoes"] = "engineered for efficient power transfer and all-day comfort",
                ["Bike Racks"] = "sturdy, secure carrying capacity for panniers and gear",
                ["Bike Stands"] = "keeps your bike upright and steady when parked or working on it",
                ["Bottles and Cages"] = "easy, secure hydration within reach on every ride",
                ["Cleaners"] = "keeps your bike running smooth and looking its best",
                ["Fenders"] = "keeps spray and grime off you and your bike in the wet",
                ["Lights"] = "bright, dependable visibility for riding after dark",
                ["Locks"] = "solid, practical security to keep your bike where you left it",
                ["Panniers"] = "weatherproof, roomy storage for commuting and touring",
                ["Pumps"] = "fast, reliable inflation whether at home or on the road",
                ["Tools"] = "the essentials for quick fixes and routine maintenance",
            };

        public static string Description(Product p, string subcategory)
        {
            var brand = Brand(p.Name);
            var blurb = subcategory != null && SubBlurb.TryGetValue(subcategory, out var b)
                ? b : "a dependable choice for everyday riding";
            var s1 = brand != null
                ? $"The {p.Name} from {brand} is {blurb}."
                : $"The {p.Name} is {blurb}.";
            var s2 = !string.IsNullOrWhiteSpace(p.Color)
                ? $"Finished in {p.Color.ToLowerInvariant()}, it balances quality with everyday value and ships ready to ride."
                : "It balances quality with everyday value and ships ready to ride.";
            return s1 + " " + s2;
        }
    }
}
