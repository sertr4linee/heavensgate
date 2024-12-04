using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace API.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;

        public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // MIME-sniffing
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            
            // clickjacking
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            
            //  XSS
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // content security policy
            context.Response.Headers.Append("Content-Security-Policy", 
                "default-src 'self'; " +
                "img-src 'self' data: https:; " +
                "font-src 'self'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "connect-src 'self';");

            // strict Transport Security
            context.Response.Headers.Append("Strict-Transport-Security", 
                "max-age=31536000; includeSubDomains");

            // permissions policy
            context.Response.Headers.Append("Permissions-Policy",
                "camera=(), microphone=(), geolocation=(), payment=()");

            try 
            {
                await _next(context);
                _logger.LogInformation("Security headers added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding security headers");
                throw;
            }
        }
    }

    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
} 