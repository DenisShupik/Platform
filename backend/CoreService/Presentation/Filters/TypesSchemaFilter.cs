using CoreService.Domain.ValueObjects;
using Microsoft.OpenApi.Models;
using SharedKernel.Domain.Interfaces;
using SharedKernel.Domain.ValueObjects;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CoreService.Presentation.Filters;

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
                case nameof(ForumId):
                case nameof(CategoryId):
                case nameof(ThreadId):
                case nameof(UserId):
                {
                    schema.Type = "string";
                    schema.Format = "uuid";
                    schema.Pattern = UuidPattern;
                    schema.Properties = null;
                    schema.Required = null;
                }
                    break;
                case nameof(PostId):
                {
                    schema.Type = "integer";
                    schema.Format = "int64";
                    schema.Minimum = 1;
                    schema.Properties = null;
                    schema.Required = null;
                }
                    break;
                case nameof(ForumTitle):
                {
                    SetStringLike<ForumTitle>(schema);
                }
                    break;
                case nameof(CategoryTitle):
                {
                    SetStringLike<CategoryTitle>(schema);
                }
                    break;
                case nameof(ThreadTitle):
                {
                    SetStringLike<ThreadTitle>(schema);
                }
                    break;
                case nameof(PostContent):
                {
                    SetStringLike<PostContent>(schema);
                }
                    break;
            }
        }
    }
}