using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Kine.Api.Middleware;

public sealed class TenantContextMiddleware
{
    private const string TenantHeaderName = "X-Tenant-Id";
    private const string TenantItemKey = "TenantId";
    private const string TenantClaimType = "tenant_id";

    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkipTenantRequirement(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var tenantId = context.User.FindFirstValue(TenantClaimType);

        if (string.IsNullOrWhiteSpace(tenantId) &&
            context.Request.Headers.TryGetValue(TenantHeaderName, out var tenantHeader))
        {
            tenantId = tenantHeader.ToString();
        }

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Tenant context missing");
            return;
        }

        context.Items[TenantItemKey] = tenantId.Trim();
        await _next(context);
    }

    private static bool ShouldSkipTenantRequirement(PathString path)
    {
        return path == "/"
            || path == "/health"
            || path.StartsWithSegments("/swagger");
    }
}
