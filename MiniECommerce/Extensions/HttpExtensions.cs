namespace MiniECommerce.Extensions
{
    public static class HttpExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest request) =>
            request.Headers["X-Requested-With"] == "XMLHttpRequest";

        /// <summary>
        /// Some URLs return a full page for normal requests and just an HTML fragment for AJAX.
        /// Tell caches (and the browser's back button) not to mix the two up.
        /// </summary>
        public static void MarkAsAjaxFragment(this HttpResponse response)
        {
            response.Headers.CacheControl = "no-store";
            response.Headers.Vary = "X-Requested-With";
        }
    }
}
