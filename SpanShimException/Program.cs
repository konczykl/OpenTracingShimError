using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Shims.OpenTracing;
using OpenTelemetry.Trace;
using OpenTracing;
using OpenTracing.Util;

const string serviceName = "SpanShimException";

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(b=> b.AddService(serviceName))
    .WithTracing(b =>
        b
            .AddSource(serviceName)
            .AddConsoleExporter()
            .SetSampler(new AlwaysOffSampler())
    )
    .StartWithHost();

builder.Services
    .AddSingleton<ITracer>(sp =>
    {
        var tracerProvider = sp.GetRequiredService<TracerProvider>();
        var tracer = new TracerShim(tracerProvider.GetTracer(serviceName), Propagators.DefaultTextMapPropagator);
        GlobalTracer.Register(tracer);
        return tracer;
    });
        
var app = builder.Build();

var tracer = app.Services.GetRequiredService<ITracer>();

using (var parent = tracer.BuildSpan("parent").StartActive())
using (var child = tracer.BuildSpan("child").StartActive())
{

}

app.Run();