using CoreService.Domain.ValueObjects;
using Microsoft.OpenApi.Models;
using NotificationService.Domain.ValueObjects;
using SharedKernel.Domain.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserService.Domain.ValueObjects;

namespace NotificationService.Presentation.Filters;

public sealed class TypesDocumentFilter : IDocumentFilter
{
    private const string UuidPattern = "^(?!00000000-0000-0000-0000-000000000000$)";
    private const string NonEmptyPattern = @"^(?!\s*$).+";

    private static void SetStringLike<T>(OpenApiSchema schema) where T : IHasMinLength, IHasMaxLength
    {
        schema.Type = "string";
        schema.MinLength = T.MinLength;
        schema.MaxLength = T.MaxLength;
        schema.Pattern = NonEmptyPattern;
        schema.Properties = null;
        schema.Required = null;
    }

    public void Apply(OpenApiDocument openApiDocument, DocumentFilterContext context)
    {
        foreach (var (key, schema) in openApiDocument.Components.Schemas)
        {
            switch (key)
            {
                case nameof(ThreadId):
                case nameof(UserId):
                case nameof(NotificationId):
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