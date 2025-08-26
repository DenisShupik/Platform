using CoreService.Domain.ValueObjects;
using Microsoft.OpenApi.Models;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.Helpers;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserService.Domain.ValueObjects;

namespace CoreService.Presentation.Filters;

public sealed class TypesDocumentFilter : IDocumentFilter
{
    private const string Suffix = "Enum";

    public void Apply(OpenApiDocument openApiDocument, DocumentFilterContext context)
    {
        var keysToRemove = new HashSet<string>();
        foreach (var (key, schema) in openApiDocument.Components.Schemas)
        {
            switch (key)
            {
                case nameof(ForumId):
                case nameof(CategoryId):
                case nameof(ThreadId):
                case nameof(PostId):
                case nameof(UserId):
                {
                    OpenApiHelper.SetUuidId(schema);
                    break;
                }
                case nameof(ForumTitle):
                {
                    OpenApiHelper.SetStringNonEmpty<ForumTitle>(schema);
                    break;
                }
                case nameof(CategoryTitle):
                {
                    OpenApiHelper.SetStringNonEmpty<CategoryTitle>(schema);
                    break;
                }
                case nameof(ThreadTitle):
                {
                    OpenApiHelper.SetStringNonEmpty<ThreadTitle>(schema);
                    break;
                }
                case nameof(PostContent):
                {
                    OpenApiHelper.SetStringNonEmpty<PostContent>(schema);
                    break;
                }
                case nameof(PaginationOffset):
                {
                    OpenApiHelper.SetPaginationOffset(schema);
                    break;
                }
                case nameof(PostIndex):
                {
                    OpenApiHelper.SetULongIndex(schema);
                    break;
                }
                case var _ when key.EndsWith("SortEnum", StringComparison.Ordinal):
                {
                    openApiDocument.Components.Schemas[key] = OpenApiHelper.CreateSortEnum(schema);
                    keysToRemove.Add(key[..^Suffix.Length] + "Type");
                    break;
                }
            }
        }

        foreach (var key in keysToRemove)
        {
            openApiDocument.Components.Schemas.Remove(key);
        }
    }
}