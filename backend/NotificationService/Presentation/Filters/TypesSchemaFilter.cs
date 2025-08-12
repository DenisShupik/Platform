using CoreService.Domain.ValueObjects;
using Microsoft.OpenApi.Models;
using NotificationService.Domain.ValueObjects;
using SharedKernel.Presentation.Helpers;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserService.Domain.ValueObjects;

namespace NotificationService.Presentation.Filters;

public sealed class TypesDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument openApiDocument, DocumentFilterContext context)
    {
        foreach (var (key, schema) in openApiDocument.Components.Schemas)
        {
            switch (key)
            {
                case nameof(ThreadId):
                case nameof(UserId):
                case nameof(NotifiableEventId):
                {
                    OpenApiHelper.SetUuidId(schema);
                    break;
                }
                case var _ when key.EndsWith("SortEnum", StringComparison.Ordinal):
                {
                    openApiDocument.Components.Schemas[key] = OpenApiHelper.CreateSortEnum(schema);
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