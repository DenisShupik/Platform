using System.Reflection;
using FluentValidation;
using JasperFx.CodeGeneration;
using Mapster;
using NotificationService.Application.Dtos;
using NotificationService.Domain.Entities;
using Wolverine;
using Wolverine.FluentValidation;

namespace NotificationService.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);

        TypeAdapterConfig.GlobalSettings.NewConfig<UserNotification, InternalUserNotificationDto>()
            .Map(dest => dest.Payload, src => src.Notification.Payload)
            .Map(dest => dest.OccurredAt, src => src.Notification.OccurredAt)
            ;
    }
}