using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Kine.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseMiddleware<TenantContextMiddleware>();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();

public partial class Program { }
