namespace MiniECommerce.Extensions
{
    public static class SecurityHeadersExtensions
    {
        // All scripts are served from this site (no inline scripts / CDNs), so a strict policy is possible.
        // img-src allows https: because admins may paste external image URLs.
        private const string ContentSecurityPolicy =
            "default-src 'self'; " +
            "img-src 'self' data: https:; " +
            "style-src 'self' 'unsafe-inline'; " +
            "script-src 'self'; " +
            "font-src 'self' data:; " +
            "connect-src 'self'; " +
            "object-src 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'; " +
            "frame-ancestors 'none'";

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, bool enableCsp)
        {
            return app.Use(async (context, next) =>
            {
                // OnStarting so our values are the last word (the antiforgery system also sets X-Frame-Options).
                context.Response.OnStarting(() =>
                {
                    var headers = context.Response.Headers;
                    headers["X-Content-Type-Options"] = "nosniff";
                    headers["X-Frame-Options"] = "DENY";
                    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

                    if (enableCsp)
                        headers["Content-Security-Policy"] = ContentSecurityPolicy;

                    return Task.CompletedTask;
                });

                await next();
            });
        }
    }
}
