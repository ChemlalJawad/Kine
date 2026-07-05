using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Kine.Api.Middleware;
using Kine.Api.Modules;
using Kine.Api.Seed;
using Kine.Api.Swagger;
using Kine.Modules.Audit.Application;
using Kine.Modules.Audit.Infrastructure;
using Kine.Modules.Billing.Application;
using Kine.Modules.Billing.Infrastructure;
using Kine.Modules.Clinical.Application;
using Kine.Modules.Clinical.Infrastructure;
using Kine.Modules.Patients.Application;
using Kine.Modules.Patients.Infrastructure;
using Kine.Modules.Reimbursement.Application;
using Kine.Modules.Reimbursement.Infrastructure;
using Kine.Modules.Scheduling.Application;
using Kine.Modules.Scheduling.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

const string LocalDevCorsPolicy = "LocalDev";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy(LocalDevCorsPolicy, policy =>
    {
        // Vite auto-increments its port (5173, 5174, ...) whenever the default is
        // taken, so pin only the scheme/host and accept any localhost port in dev.
        policy
            .SetIsOriginAllowed(origin =>
                Uri.TryCreate(origin, UriKind.Absolute, out var originUri) &&
                originUri.Scheme is "http" or "https" &&
                originUri.Host is "localhost" or "127.0.0.1")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddSingleton<IPatientStore, InMemoryPatientStore>();
builder.Services.AddSingleton<PatientService>();
builder.Services.AddSingleton<ISchedulingStore, InMemorySchedulingStore>();
builder.Services.AddSingleton<SchedulingService>();
builder.Services.AddSingleton<IInvoiceStore, InMemoryInvoiceStore>();
builder.Services.AddSingleton<BillingService>();
builder.Services.AddSingleton<ISeanceStore, InMemorySeanceStore>();
builder.Services.AddSingleton<IPrescriptionStore, InMemoryPrescriptionStore>();
builder.Services.AddSingleton<ClinicalService>();
builder.Services.AddSingleton<IReimbursementCaseStore, InMemoryReimbursementCaseStore>();
builder.Services.AddSingleton<ReimbursementService>();
builder.Services.AddSingleton<IAuditLogStore, InMemoryAuditLogStore>();
builder.Services.AddSingleton<AuditTrailService>();
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

// Stores are in-memory only for now (no DB wired in yet, see CLAUDE.md), so every
// process start begins empty. Always seed demo data -- there is no persistent
// "production" state yet for this to leak into.
DemoDataSeeder.Seed(
    app.Services.GetRequiredService<IPatientStore>(),
    app.Services.GetRequiredService<ISchedulingStore>(),
    app.Services.GetRequiredService<IInvoiceStore>(),
    app.Services.GetRequiredService<ISeanceStore>(),
    app.Services.GetRequiredService<IPrescriptionStore>(),
    app.Services.GetRequiredService<IReimbursementCaseStore>());

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(LocalDevCorsPolicy);

// Outermost business middleware: maps domain exceptions from every endpoint to
// 400/404/409 with the shared { error } body (see ExceptionMappingMiddleware).
app.UseMiddleware<ExceptionMappingMiddleware>();
app.UseMiddleware<StaffMfaEnforcementMiddleware>();
app.UseMiddleware<TenantContextMiddleware>();
app.UseMiddleware<RbacMiddleware>();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapPatientsEndpoints();
app.MapSchedulingEndpoints();
app.MapBillingEndpoints();
app.MapClinicalEndpoints();
app.MapReimbursementEndpoints();
app.MapReportingEndpoints();
app.MapAuditEndpoints();

app.Run();

public partial class Program { }
