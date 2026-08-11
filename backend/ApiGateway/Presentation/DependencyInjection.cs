using Shared.Presentation.Extensions;

namespace ApiGateway.Presentation;

public static class DependencyInjection
{
    public static void AddPresentationServices(this IHostApplicationBuilder builder)
    {
        builder.Services
            .RegisterApiLocalization()
            .AddServiceHealthChecks();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost",
                    b => b.WithOrigins("http://localhost:4173", "http://localhost:5173")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
        }
    }
}
