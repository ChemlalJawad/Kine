using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Kine.Api.Security;
using Microsoft.AspNetCore.Http;

namespace Kine.Api.Middleware;

/// <summary>
/// RBAC per cabinet (P0-006). Extracts roles from OIDC role claims when the
/// request is authenticated, otherwise from the X-Roles header (comma-separated,
/// dev/demo fallback aligned with the X-Tenant-Id pattern). Enforces a static
/// permission matrix per API area and HTTP method (GET = read, others = write).
///
/// MVP behavior mirrors StaffMfaEnforcementMiddleware: requests carrying NO role
/// information at all are let through (local dev without IdP). As soon as role
/// information is present, the matrix is enforced and insufficient roles get 403.
/// Real enforcement for all requests activates with the OIDC integration
/// (P0-007/Identity), where every staff request carries role claims.
/// </summary>
public sealed class RbacMiddleware
{
    public const string RolesItemKey = "Roles";
    private const string RolesHeaderName = "X-Roles";

    private sealed record AreaPolicy(string Prefix, string[] ReadRoles, string[] WriteRoles);

    private static readonly AreaPolicy[] Policies =
    {
        new("/api/patients",
            ReadRoles: RoleNames.All,
            WriteRoles: new[] { RoleNames.AdminCabinet, RoleNames.Kine, RoleNames.Assistant }),
        new("/api/scheduling",
            ReadRoles: RoleNames.All,
            WriteRoles: new[] { RoleNames.AdminCabinet, RoleNames.Kine, RoleNames.Assistant }),
        new("/api/clinical",
            ReadRoles: new[] { RoleNames.AdminCabinet, RoleNames.Kine, RoleNames.Assistant },
            WriteRoles: new[] { RoleNames.AdminCabinet, RoleNames.Kine }),
        new("/api/billing",
            ReadRoles: new[] { RoleNames.AdminCabinet, RoleNames.Billing },
            WriteRoles: new[] { RoleNames.AdminCabinet, RoleNames.Billing }),
        new("/api/reimbursement",
            ReadRoles: new[] { RoleNames.AdminCabinet, RoleNames.Billing },
            WriteRoles: new[] { RoleNames.AdminCabinet, RoleNames.Billing }),
        new("/api/reporting",
            ReadRoles: new[] { RoleNames.AdminCabinet },
            WriteRoles: new[] { RoleNames.AdminCabinet }),
        new("/api/audit",
            ReadRoles: new[] { RoleNames.AdminCabinet },
            WriteRoles: new[] { RoleNames.AdminCabinet })
    };

    private readonly RequestDelegate _next;

    public RbacMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var roles = ExtractRoles(context);
        if (roles.Count > 0)
        {
            context.Items[RolesItemKey] = roles;
        }

        var policy = Policies.FirstOrDefault(p =>
            context.Request.Path.StartsWithSegments(p.Prefix, StringComparison.OrdinalIgnoreCase));

        if (policy is null || roles.Count == 0)
        {
            // No protected area matched, or no role information supplied (local dev
            // without IdP) -- see class doc; tenant scoping still applies downstream.
            await _next(context);
            return;
        }

        var requiredRoles = HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method)
            ? policy.ReadRoles
            : policy.WriteRoles;

        if (!roles.Any(role => requiredRoles.Contains(role, StringComparer.OrdinalIgnoreCase)))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                error = $"Insufficient role for {context.Request.Method} {policy.Prefix}. Required: {string.Join("|", requiredRoles)}."
            });
            return;
        }

        await _next(context);
    }

    private static IReadOnlyList<string> ExtractRoles(HttpContext context)
    {
        // OIDC role claims take precedence when the request is authenticated.
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var claimRoles = context.User.Claims
                .Where(claim => claim.Type is ClaimTypes.Role or "role" or "roles")
                .SelectMany(claim => claim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (claimRoles.Count > 0)
            {
                return claimRoles;
            }
        }

        if (context.Request.Headers.TryGetValue(RolesHeaderName, out var headerValue))
        {
            return headerValue.ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return Array.Empty<string>();
    }
}
