using System.Reflection;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Clients.Api.Diagnostics;

public static class OpenTelemetryConfigurationExtensions
{
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService("Clients.Api", "Test.Course.OpenTelemetry",
                    Assembly.GetExecutingAssembly().GetName().Version!.ToString())
                .AddAttributes([
                    new KeyValuePair<string, object>("service.environment",
                        builder.Environment.EnvironmentName)
                ])
            )
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddNpgsql()
                .AddGrpcClientInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRedisInstrumentation()
                .AddConsoleExporter()
                .AddOtlpExporter(options =>
                    options.Endpoint = new Uri(builder.Configuration.GetValue<string>("Jaeger")!))
            )
            .WithMetrics(metrics => metrics
                .AddMeter(ApplicationDiagnostics.Meter.Name)
                .AddConsoleExporter()
            );

        return builder;
    }
}