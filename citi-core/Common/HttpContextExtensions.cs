using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace citi_core.Common
{
    public static class HttpContextExtensions
    {
        public static string GetCorrelationId(this HttpContext context) =>
            context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        public static string GetUserAgent(this HttpContext context) =>
            context.Request.Headers["User-Agent"].ToString();

        public static string GetIpAddress(this HttpContext context) =>
            context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        public static Guid? GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst("sub")
                      ?? user.FindFirst(ClaimTypes.NameIdentifier)
                      ?? user.FindFirst("userId");

            return claim != null ? Guid.Parse(claim.Value) : null;
        }
    }
}