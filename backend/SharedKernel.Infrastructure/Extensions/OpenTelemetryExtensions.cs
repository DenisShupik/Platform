using System.Diagnostics;
using LinqToDB.Metrics;
using OpenTelemetry.Trace;

namespace SharedKernel.Infrastructure.Extensions;

public static class OpenTelemetryExtensions
{
    private const string SourceName = "LinqToDB";
    private static readonly ActivitySource ActivitySource = new(SourceName);
    
    public static TracerProviderBuilder AddLinqToDbInstrumentation(this TracerProviderBuilder builder)
    {
        ActivityService.AddFactory(LinqToDbActivity.Create);
        builder.AddSource(SourceName);
        return builder;
    }

    private sealed class LinqToDbActivity : ActivityBase
    {
        private readonly Activity _activity;

        private LinqToDbActivity(ActivityID id, Activity activity) : base(id)
        {
            _activity = activity;
        }

        public override void Dispose()
        {
            _activity.Dispose();
        }
        
        public static IActivity? Create(ActivityID id)
        {
            var activity = ActivitySource.StartActivity(id.ToString());
            return activity == null ? null : new LinqToDbActivity(id, activity);
        }
    }
}