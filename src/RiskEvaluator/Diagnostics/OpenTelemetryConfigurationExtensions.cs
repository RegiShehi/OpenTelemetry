using System.Reflection;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace RiskEvaluator.Diagnostics;

public static class OpenTelemetryConfigurationExtensions
{
    public static IServiceCollection AddOpenTelemetryConfig(this IServiceCollection services)
    {
        const string serviceName = "RiskEvaluator";

        var otlpEndpoint = new Uri(Environment.GetEnvironmentVariable("OTLP_Endpoint")!);

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName)
                .AddAttributes([
                        new KeyValuePair<string, object>("service.environment",
                            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"),
                        new KeyValuePair<string, object>("service.version",
                            Assembly.GetExecutingAssembly().GetName().Version!.ToString())
                    ]
                )
            )
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                // .AddConsoleExporter()
                .AddOtlpExporter(options =>
                    options.Endpoint = otlpEndpoint)
            )
            .WithLogging(logging => logging
                // .AddConsoleExporter()
                .AddOtlpExporter(options =>
                    options.Endpoint = otlpEndpoint)
            );

        return services;
    }
}