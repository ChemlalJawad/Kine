using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Kine.Api.Middleware;
using Kine.Api.Modules;
using Kine.Modules.Patients.Application;
using Kine.Modules.Patients.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IPatientStore, InMemoryPatientStore>();
builder.Services.AddSingleton<PatientService>();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Kine.Api",
        Version = "v1"
    });
});
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<StaffMfaEnforcementMiddleware>();
app.UseMiddleware<TenantContextMiddleware>();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapPatientsEndpoints();

app.Run();

public partial class Program { }
