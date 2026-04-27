using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Shared.Presentation.Extensions;

public static class EndpointConventionBuilderExtensions
{
    public static TBuilder WithAutoNames<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(endpointBuilder =>
        {
            var methodInfo = endpointBuilder.Metadata.OfType<MethodInfo>().FirstOrDefault();
            if (methodInfo != null)
            {
                endpointBuilder.Metadata.Add(new EndpointNameMetadata(methodInfo.Name));
            }
            else
            {
                throw new InvalidOperationException("No MethodInfo found");
            }
        });

        return builder;
    }
}