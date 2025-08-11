using Microsoft.EntityFrameworkCore;
using TickerQ.EntityFrameworkCore.Configurations;

namespace SharedKernel.Infrastructure.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyTickerQConfiguration(this ModelBuilder builder, string schemaName)
    {
        var tickerSchemaName = schemaName + "_ticker";

        builder.ApplyConfiguration(new TimeTickerConfigurations(tickerSchemaName));
        builder.ApplyConfiguration(new CronTickerConfigurations(tickerSchemaName));
        builder.ApplyConfiguration(new CronTickerOccurrenceConfigurations(tickerSchemaName));
    }
}