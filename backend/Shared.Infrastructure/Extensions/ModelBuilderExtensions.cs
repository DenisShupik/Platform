using Microsoft.EntityFrameworkCore;
using TickerQ.EntityFrameworkCore.Configurations;
using TickerQ.Utilities.Entities;

namespace Shared.Infrastructure.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyTickerQConfiguration(this ModelBuilder builder, string schemaName)
    {
        var tickerSchemaName = schemaName + "-ticker";
        builder.ApplyConfiguration(new TimeTickerConfigurations<TimeTickerEntity>(tickerSchemaName));
        builder.ApplyConfiguration(new CronTickerConfigurations<CronTickerEntity>(tickerSchemaName));
        builder.ApplyConfiguration(new CronTickerOccurrenceConfigurations<CronTickerEntity>(tickerSchemaName));
    }
}