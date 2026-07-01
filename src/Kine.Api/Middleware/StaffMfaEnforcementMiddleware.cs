using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Kine.Api.Middleware;

/// <summary>
/// Enforces MFA for authenticated staff requests using standard OIDC claims
/// (amr = Authentication Methods References, acr = Authentication Context Class Reference).
/// No local MFA state is stored; the gate only trusts claims issued by the OIDC provider.
/// </summary>
public sealed class StaffMfaEnforcementMiddleware
{
    private const string AmrClaimType = "amr";
    private const string AcrClaimType = "acr";

    private static readonly string[] MfaAmrValues =
    {
        "mfa", "otp", "hwk", "sms", "totp", "swk"
    };

    private static readonly string[] MfaAcrIndicators =
    {
        "mfa"
    };

    private readonly RequestDelegate _next;

    public StaffMfaEnforcementMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true && !HasMfaEvidence(context.User))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("MFA required for staff access");
            return;
        }

        await _next(context);
    }

    private static bool HasMfaEvidence(ClaimsPrincipal user)
    {
        var hasMfaAmr = user.FindAll(AmrClaimType)
            .Any(claim => MfaAmrValues.Contains(claim.Value, StringComparer.OrdinalIgnoreCase));

        if (hasMfaAmr)
        {
            return true;
        }

        var acrValue = user.FindFirstValue(AcrClaimType);

        return !string.IsNullOrWhiteSpace(acrValue) &&
               MfaAcrIndicators.Any(indicator =>
                   acrValue.Contains(indicator, StringComparison.OrdinalIgnoreCase));
    }
}
