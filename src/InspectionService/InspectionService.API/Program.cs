using InspectionService.API.Endpoints;
using InspectionService.Application.Handlers;
using InspectionService.Domain.Interfaces;
using InspectionService.Infrastructure.Data;
using InspectionService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Inspection Service API",
        Version = "v1",
        Description = "Handles property inspections and photo uploads for the Intelligent Property Inspection Platform"
    });
});

// Database — SQLite for demo, SQL Server for production
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString) || connectionString.Contains(".db"))
{
    builder.Services.AddDbContext<InspectionDbContext>(options =>
        options.UseSqlite(connectionString ?? "Data Source=inspection.db"));
}
else
{
    builder.Services.AddDbContext<InspectionDbContext>(options =>
        options.UseSqlServer(connectionString));
}

builder.Services.AddScoped<IInspectionRepository, InspectionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateInspectionHandler).Assembly);
});

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<InspectionDbContext>();

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("InspectionService"))
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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inspection Service API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors("AllowAll");
app.MapHealthChecks("/health");
app.MapInspectionEndpoints();

app.MapGet("/info", () => new
{
    Service = "Inspection Service",
    Version = "1.0.0",
    Status = "Running",
    Timestamp = DateTime.UtcNow
});

// Apply migrations and seed
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InspectionDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
