using Microsoft.OpenApi.Models;
using SharedKernel.Presentation.Helpers;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserService.Domain.ValueObjects;

namespace UserService.Presentation.Filters;

public sealed class TypesDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument openApiDocument, DocumentFilterContext context)
    {
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
            }
        }
    }
}