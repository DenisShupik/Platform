using CoreService.Application.Authorization;
using CoreService.Application.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoreService.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Singleton);

        builder.Services.AddScoped<IForumPolicyEvaluator, ForumPolicyEvaluator>();
        builder.Services.AddScoped<IAuthorizationScopeResolver, AuthorizationScopeResolver>();
        builder.Services.AddSingleton<IBookmarkPolicyEvaluator, BookmarkPolicyEvaluator>();

        builder.Services.RegisterHandlers();
    }
}
