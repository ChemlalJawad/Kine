using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Kine.Api.Middleware;

/// <summary>
/// Global exception-to-status mapping for the whole API surface, replacing the
/// per-endpoint try/catch duplication:
///   ArgumentException          -> 400 Bad Request
///   KeyNotFoundException       -> 404 Not Found
///   InvalidOperationException  -> 409 Conflict
/// The response body keeps the existing `{ "error": message }` contract used by
/// the frontend httpClient and the integration tests. Anything else bubbles up
/// as a 500 through the default pipeline.
/// </summary>
public sealed class ExceptionMappingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMappingMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex) when (!context.Response.HasStarted)
        {
            await WriteError(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (KeyNotFoundException ex) when (!context.Response.HasStarted)
        {
            await WriteError(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (InvalidOperationException ex) when (!context.Response.HasStarted)
        {
            await WriteError(context, StatusCodes.Status409Conflict, ex.Message);
        }
    }

    private static Task WriteError(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new { error = message });
    }
}
