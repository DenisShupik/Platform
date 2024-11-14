using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SharedKernel.Filters;

public sealed class AddOperationIdOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodName = context.MethodInfo.Name;

        if (methodName.EndsWith("Async", StringComparison.OrdinalIgnoreCase))
        {
            methodName = methodName[..^5];
        }

        operation.OperationId = char.ToLower(methodName[0]) + methodName[1..];
    }
}