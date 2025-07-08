using Microsoft.EntityFrameworkCore;
using TickerQ.EntityFrameworkCore.Configurations;
using TickerQ.EntityFrameworkCore.Entities;

namespace SharedKernel.Infrastructure.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyTickerQConfiguration(this ModelBuilder builder, string schemaName)
    {
        var tickerSchemaName = schemaName + "_ticker";

        builder.Entity<TimeTickerEntity>(b =>
        {
            new TimeTickerConfigurations().Configure(b);
            b.ToTable("time_tickers", tickerSchemaName);
        });

        builder.Entity<CronTickerEntity>(b =>
        {
            new CronTickerConfigurations().Configure(b);
            b.ToTable("cron_tickers", tickerSchemaName);
        });

        builder.Entity<CronTickerOccurrenceEntity<CronTickerEntity>>(b =>
        {
            new CronTickerOccurrenceConfigurations().Configure(b);
            b.ToTable("cron_ticker_occurrences", tickerSchemaName);
        });
    }
}
