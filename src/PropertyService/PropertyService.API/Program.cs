using Microsoft.EntityFrameworkCore;
using PropertyService.Infrastructure.Data;
using PropertyService.Infrastructure.Repositories;
using PropertyService.Infrastructure.Services;
using PropertyService.Domain.Interfaces;
using PropertyService.Application.Interfaces;
using Azure.Messaging.ServiceBus;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Property Service API",
        Version = "v1",
        Description = "Manages property records for the Intelligent Property Inspection Platform"
    });
});

// Database — SQLite for demo, SQL Server for production
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("memory") || connectionString.Contains(".db"))
{
    builder.Services.AddDbContext<PropertyDbContext>(options =>
        options.UseSqlite(connectionString ?? "Data Source=property.db"));
}
else
{
    builder.Services.AddDbContext<PropertyDbContext>(options =>
        options.UseSqlServer(connectionString));
}

builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Event publisher — use real Service Bus if configured, else no-op
var serviceBusConnection = builder.Configuration.GetConnectionString("ServiceBus");
if (!string.IsNullOrEmpty(serviceBusConnection))
{
    builder.Services.AddSingleton(new ServiceBusClient(serviceBusConnection));
    builder.Services.AddScoped<IEventPublisher, ServiceBusEventPublisher>();
}
else
{
    builder.Services.AddScoped<IEventPublisher, NoOpEventPublisher>();
}

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(PropertyService.Application.Handlers.CreatePropertyHandler).Assembly);
});

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PropertyDbContext>();

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("PropertyService"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Property Service API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors("AllowAll");
app.MapControllers();
app.MapHealthChecks("/health");

app.MapGet("/info", () => new
{
    Service = "Property Service",
    Version = "1.0.0",
    Status = "Running",
    Timestamp = DateTime.UtcNow
});

// Apply migrations and seed
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PropertyDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
