using CoreService.Domain.ValueObjects;
using Microsoft.OpenApi.Models;
using SharedKernel.Domain.Interfaces;
using SharedKernel.Presentation.Helpers;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserService.Domain.ValueObjects;

namespace CoreService.Presentation.Filters;

public sealed class TypesDocumentFilter : IDocumentFilter
{
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
                    OpenApiHelper.SetUuidId(schema);
                    break;
                }
                case nameof(PostId):
                {
                    OpenApiHelper.SetLongId(schema);
                    break;
                }
                case nameof(ForumTitle):
                {
                    OpenApiHelper.SetStringLike<ForumTitle>(schema);
                    break;
                }
                case nameof(CategoryTitle):
                {
                    OpenApiHelper.SetStringLike<CategoryTitle>(schema);
                    break;
                }
                case nameof(ThreadTitle):
                {
                    OpenApiHelper.SetStringLike<ThreadTitle>(schema);
                    break;
                }
                case nameof(PostContent):
                {
                    OpenApiHelper.SetStringLike<PostContent>(schema);
                    break;
                }
                case var _ when key.EndsWith("SortEnum", StringComparison.Ordinal):
                {
                    openApiDocument.Components.Schemas[key] = OpenApiHelper.CreateSortEnum(schema);
                    break;
                }
            }
        }
    }
}