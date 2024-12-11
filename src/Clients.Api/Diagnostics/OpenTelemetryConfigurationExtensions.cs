using System.Reflection;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Clients.Api.Diagnostics;

public static class OpenTelemetryConfigurationExtensions
{
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        var otlpEndpoint = new Uri(builder.Configuration.GetValue<string>("OTLP_Endpoint")!);

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
                // .AddConsoleExporter()
                .AddOtlpExporter(options =>
                    options.Endpoint = otlpEndpoint)
            )
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddMeter(ApplicationDiagnostics.Meter.Name)
                // .AddConsoleExporter()
                .AddOtlpExporter(options => options.Endpoint = otlpEndpoint)
            )
            .WithLogging(logging => logging
                // .AddConsoleExporter()
                .AddOtlpExporter(options =>
                    options.Endpoint = otlpEndpoint)
            );

        return builder;
    }
}