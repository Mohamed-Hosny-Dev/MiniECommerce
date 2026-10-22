using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace MiniECommerce.Extensions
{
    public static class FormatExtensions
    {
        private const string PlaceholderImage = "~/images/default-product.svg";

        private static readonly Lazy<TimeZoneInfo> CairoZone = new(() =>
        {
            foreach (var id in new[] { "Africa/Cairo", "Egypt Standard Time" })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }
            return TimeZoneInfo.Utc;
        });

        /// <summary>Culture independent so the server output always matches cart.js (e.g. "1,299.00 EGP").</summary>
        public static string ToEgp(this decimal amount) =>
            amount.ToString("N2", CultureInfo.InvariantCulture) + " EGP";

        /// <summary>Dates are stored in UTC; show them to the customer in Cairo time.</summary>
        public static DateTime ToCairoTime(this DateTime utc)
        {
            var asUtc = utc.Kind == DateTimeKind.Local
                ? utc.ToUniversalTime()
                : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(asUtc, CairoZone.Value);
        }

        public static string ToCairoString(this DateTime utc, string format = "dd MMM yyyy, HH:mm") =>
            utc.ToCairoTime().ToString(format, CultureInfo.InvariantCulture);

        /// <summary>
        /// Resolves a product image to a URL that works from any page and any PathBase.
        /// Fixes the old relative "default-product.png" which broke on /Products/Details/5.
        /// </summary>
        public static string ProductImage(this IUrlHelper url, string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) || imageUrl == "default-product.png")
                return url.Content(PlaceholderImage);

            imageUrl = imageUrl.Trim();

            if (imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                return imageUrl;

            if (imageUrl.StartsWith("//"))
                return url.Content(PlaceholderImage);

            if (imageUrl.StartsWith("~/"))
                return url.Content(imageUrl);

            return url.Content("~/" + imageUrl.TrimStart('/'));
        }

        public static string CategoryIcon(string? name)
        {
            var n = (name ?? string.Empty).ToLowerInvariant();

            bool Has(params string[] keys) => keys.Any(k => n.Contains(k));

            if (Has("electronic", "phone", "mobile", "laptop", "computer", "gadget", "إلكترون", "الكترون", "موبايل", "هاتف"))
                return "fa-laptop";
            if (Has("fashion", "cloth", "wear", "shirt", "shoe", "apparel", "ملابس", "أزياء", "ازياء"))
                return "fa-shirt";
            if (Has("home", "kitchen", "furniture", "decor", "منزل", "مطبخ", "أثاث", "اثاث"))
                return "fa-couch";
            if (Has("beauty", "cosmetic", "care", "skin", "perfume", "تجميل", "عطور"))
                return "fa-spa";
            if (Has("book", "stationery", "كتب"))
                return "fa-book-open";
            if (Has("sport", "fitness", "gym", "رياض"))
                return "fa-dumbbell";
            if (Has("toy", "kid", "baby", "child", "أطفال", "اطفال", "ألعاب أطفال"))
                return "fa-baby-carriage";
            if (Has("game", "gaming", "العاب", "ألعاب"))
                return "fa-gamepad";
            if (Has("food", "grocer", "snack", "غذ", "بقال"))
                return "fa-basket-shopping";
            if (Has("jewel", "watch", "accessor", "مجوهرات", "ساع"))
                return "fa-gem";
            if (Has("car", "auto", "سيار"))
                return "fa-car";

            return "fa-tag";
        }
    }
}
