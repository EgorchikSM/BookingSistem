using BookingSistem.Services;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IMetricsService, MetricsService>();
builder.Services.AddSingleton<IBookingService, BookingService>();

builder.Services.AddHealthChecks()
    .AddCheck<ReadinessHealthCheck>("ready");

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

try
{
    app.MapControllers();
}
catch (ReflectionTypeLoadException ex)
{
    foreach (var loaderException in ex.LoaderExceptions)
    {
        Console.WriteLine(loaderException?.Message);
    }
    throw;
}

app.MapHealthChecks("/health/live");

app.MapHealthChecks("/health/ready");

app.Run();