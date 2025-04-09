using Microsoft.OpenApi.Models;
using SharedKernel.Domain.ValueObjects;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UserService.Presentation.Filters;

public sealed class TypesDocumentFilter : IDocumentFilter
{
    private const string UuidPattern = "^(?!00000000-0000-0000-0000-000000000000$)";

    public void Apply(OpenApiDocument openApiDocument, DocumentFilterContext context)
    {
        foreach (var (key, schema) in openApiDocument.Components.Schemas)
        {
            switch (key)
            {
                case nameof(UserId):
                {
                    schema.Type = "string";
                    schema.Format = "uuid";
                    schema.Pattern = UuidPattern;
                    schema.Properties = null;
                    schema.Required = null;
                }
                    break;
            }
        }
    }
}