using System.Diagnostics;

namespace Accessory.Builder.Logging.OpenTelemetry.Clients;

public class TelemetrySource
{
    public TelemetrySource(string activitySourceName)
    {
        Source = new ActivitySource(activitySourceName);
    }

    public ActivitySource Source { get; }
}