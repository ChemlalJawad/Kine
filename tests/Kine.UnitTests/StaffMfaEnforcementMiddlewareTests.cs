using System.Security.Claims;
using System.Threading.Tasks;
using Kine.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Kine.UnitTests;

public class StaffMfaEnforcementMiddlewareTests
{
    [Fact]
    public async Task Unauthenticated_request_passes_through()
    {
        var nextCalled = false;
        var middleware = new StaffMfaEnforcementMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task Authenticated_staff_without_mfa_claim_is_rejected()
    {
        var nextCalled = false;
        var middleware = new StaffMfaEnforcementMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "staff-user")
            }, "oidc"))
        };

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task Authenticated_staff_with_mfa_amr_claim_passes_through()
    {
        var nextCalled = false;
        var middleware = new StaffMfaEnforcementMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "staff-user"),
                new Claim("amr", "mfa")
            }, "oidc"))
        };

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Authenticated_staff_with_mfa_acr_claim_passes_through()
    {
        var nextCalled = false;
        var middleware = new StaffMfaEnforcementMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "staff-user"),
                new Claim("acr", "urn:example:loa-mfa")
            }, "oidc"))
        };

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }
}
