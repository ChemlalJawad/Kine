using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Kine.Api.Middleware;
using Kine.Api.Modules;
using Kine.Api.Swagger;
using Kine.Modules.Patients.Application;
using Kine.Modules.Patients.Infrastructure;
using Kine.Modules.Scheduling.Application;
using Kine.Modules.Scheduling.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

const string LocalDevCorsPolicy = "LocalDev";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy(LocalDevCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddSingleton<IPatientStore, InMemoryPatientStore>();
builder.Services.AddSingleton<PatientService>();
builder.Services.AddSingleton<ISchedulingStore, InMemorySchedulingStore>();
builder.Services.AddSingleton<SchedulingService>();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Kine.Api",
        Version = "v1"
    });
    options.OperationFilter<TenantHeadersOperationFilter>();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(LocalDevCorsPolicy);

app.UseMiddleware<StaffMfaEnforcementMiddleware>();
app.UseMiddleware<TenantContextMiddleware>();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapPatientsEndpoints();
app.MapSchedulingEndpoints();

app.Run();

public partial class Program { }
