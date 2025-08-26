using Microsoft.OpenApi.Models;
using SharedKernel.Presentation.Helpers;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserService.Domain.ValueObjects;

namespace UserService.Presentation.Filters;

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
                case nameof(UserId):
                {
                    OpenApiHelper.SetUuidId(schema);
                    break;
                }
                case nameof(Username):
                {
                    OpenApiHelper.SetPatternString<Username>(schema);
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