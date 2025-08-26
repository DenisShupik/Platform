using CoreService.Domain.ValueObjects;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;
using SharedKernel.Presentation.Helpers;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserService.Domain.ValueObjects;

namespace NotificationService.Presentation.Filters;

public sealed class TypesDocumentFilter : IDocumentFilter
{
    private const string Suffix = "Enum";
    public void Apply(OpenApiDocument openApiDocument, DocumentFilterContext context)
    {
        var enumValues = Enum.GetValues<NotifiableEventPayloadType>();

        var varNamesArray = new OpenApiArray();
        varNamesArray.AddRange(enumValues.Select(value => new OpenApiString(value.ToString())));

        var enumSchema = new OpenApiSchema
        {
            Type = "string",
            Description = "Типы уведомлений",
            Enum = enumValues.Select(IOpenApiAny (value) => new OpenApiString(value.ToString("G"))).ToList(),
            Extensions =
            {
                ["x-enum-varnames"] = varNamesArray
            }
        };

        openApiDocument.Components.Schemas[nameof(NotifiableEventPayloadType)] = enumSchema;

        var keysToRemove = new HashSet<string>();
        foreach (var (key, schema) in openApiDocument.Components.Schemas)
        {
            switch (key)
            {
                case nameof(CategoryId):
                case nameof(ThreadId):
                case nameof(PostId):
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
                case nameof(NotifiableEventPayload):
                {
                    foreach (var payloadType in enumValues)
                    {
                        var schemaName = $"{payloadType}NotifiableEventPayload";
                        schema.Discriminator.Mapping[payloadType.ToString("G")] = $"#/components/schemas/{schemaName}";
                    }

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