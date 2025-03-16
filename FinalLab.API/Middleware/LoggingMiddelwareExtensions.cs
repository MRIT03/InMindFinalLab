// File: Microservices/Logger/Middleware/LoggingMiddlewareExtensions.cs
using Microsoft.AspNetCore.Builder;

namespace FinalLab.API.Middleware
{
    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LoggingMiddleware>();
        }
    }
}