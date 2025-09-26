using FluentValidation;
using Mapster;
using NotificationService.Application.Dtos;
using NotificationService.Domain.Entities;

namespace NotificationService.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Singleton);

        builder.Services.RegisterHandlers();

        TypeAdapterConfig.GlobalSettings.NewConfig<Notification, InternalNotificationDto>()
            .Map(dest => dest.Payload, src => src.NotifiableEvent.Payload)
            .Map(dest => dest.OccurredAt, src => src.NotifiableEvent.OccurredAt);
    }
}