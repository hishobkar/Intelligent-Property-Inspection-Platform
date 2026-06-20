using DocumentService.Application.Handlers;
using DocumentService.Domain.Interfaces;
using DocumentService.Infrastructure.Data;
using DocumentService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Document Service API",
        Version = "v1",
        Description = "Generates compliance and inspection reports for the Intelligent Property Inspection Platform"
    });
});

// Database — SQLite for demo, SQL Server for production
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString) || connectionString.Contains(".db"))
{
    builder.Services.AddDbContext<DocumentDbContext>(options =>
        options.UseSqlite(connectionString ?? "Data Source=document.db"));
}
else
{
    builder.Services.AddDbContext<DocumentDbContext>(options =>
        options.UseSqlServer(connectionString));
}

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GenerateComplianceReportHandler).Assembly);
});

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<DocumentDbContext>();

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("DocumentService"))
        .AddAspNetCoreInstrumentation());

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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Document Service API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors("AllowAll");
app.MapControllers();
app.MapHealthChecks("/health");

app.MapGet("/info", () => new
{
    Service = "Document Service",
    Version = "1.0.0",
    Status = "Running",
    Timestamp = DateTime.UtcNow
});

// Apply migrations and seed
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DocumentDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
