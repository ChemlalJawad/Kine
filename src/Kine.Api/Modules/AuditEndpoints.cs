using Kine.Modules.Audit.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Kine.Api.Modules;

/// <summary>
/// Read-only HTTP surface for the Audit module: consultation du journal
/// append-only et verification d'integrite de la chaine de hash (P0-008).
/// Reserved to AdminCabinet by the RBAC middleware.
/// </summary>
public static class AuditEndpoints
{
    private const string TenantItemKey = "TenantId";

    public static void MapAuditEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/audit");

        group.MapGet("/events", (IAuditLogStore store, HttpContext context) =>
        {
            return Results.Ok(store.GetChain(Tenant(context)));
        });

        group.MapGet("/verify", (IAuditLogStore store, HttpContext context) =>
        {
            var chain = store.GetChain(Tenant(context));
            return Results.Ok(new { count = chain.Count, isValid = AuditChainVerifier.IsValid(chain) });
        });
    }

    private static string Tenant(HttpContext context) => (string)context.Items[TenantItemKey]!;
}
