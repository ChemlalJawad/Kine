using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kine.Api.Swagger;

public sealed class TenantHeadersOperationFilter : IOperationFilter
{
    private const string TenantHeaderName = "X-Tenant-Id";
    private const string ActorHeaderName = "X-Actor-Id";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var relativePath = context.ApiDescription.RelativePath;

        if (string.IsNullOrWhiteSpace(relativePath) ||
            !relativePath.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        operation.Parameters ??= new List<OpenApiParameter>();

        AddHeaderIfMissing(operation.Parameters, new OpenApiParameter
        {
            Name = TenantHeaderName,
            In = ParameterLocation.Header,
            Required = true,
            Description = "Tenant context required by all business endpoints.",
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new OpenApiString("tenant-demo")
            }
        });

        AddHeaderIfMissing(operation.Parameters, new OpenApiParameter
        {
            Name = ActorHeaderName,
            In = ParameterLocation.Header,
            Required = false,
            Description = "Actor identifier used for audit and mutations. Optional in demo mode.",
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new OpenApiString("staff-1")
            }
        });

        operation.Responses.TryAdd("400", new OpenApiResponse
        {
            Description = "Bad Request when tenant context is missing or the request payload is invalid.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["text/plain"] = new(),
                ["application/json"] = new()
            }
        });
    }

    private static void AddHeaderIfMissing(ICollection<OpenApiParameter> parameters, OpenApiParameter parameter)
    {
        if (parameters.Any(existing =>
                existing.In == parameter.In &&
                string.Equals(existing.Name, parameter.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        parameters.Add(parameter);
    }
}