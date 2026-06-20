# Intelligent Property Inspection Platform — Complete Technical Guide

> **Who is this for?**
> A .NET fresher who knows basic C# but has little experience with microservices, cloud, or DevOps.
> Every concept is explained from first principles before diving into how *this project* uses it.

---

## Table of Contents

1. [What Does This Project Do?](#1-what-does-this-project-do)
2. [What Is a Microservice?](#2-what-is-a-microservice)
3. [The Five Services — What Each One Does](#3-the-five-services)
4. [How the Code Is Organised (Clean Architecture)](#4-how-the-code-is-organised-clean-architecture)
5. [The CQRS Pattern (Commands and Queries)](#5-the-cqrs-pattern)
6. [How Data Is Stored (Entity Framework Core + SQLite)](#6-how-data-is-stored)
7. [How Services Talk to Each Other (Events)](#7-how-services-talk-to-each-other)
8. [The API Layer — How Requests Come In](#8-the-api-layer)
9. [The Python Services (FastAPI)](#9-the-python-services)
10. [Docker — Packaging and Running Everything](#10-docker)
11. [How Azure Fits In (Production Mode)](#11-how-azure-fits-in)
12. [The Full Operational Flow — Step by Step](#12-the-full-operational-flow)
13. [Configuration — Demo Mode vs Production Mode](#13-configuration)
14. [The Demo Dashboard](#14-the-demo-dashboard)
15. [Observability — Health Checks and Tracing](#15-observability)
16. [Glossary](#16-glossary)

---

## 1. What Does This Project Do?

Imagine a company that inspects properties (houses, offices, shops) before they are sold, insured, or rented. They need software to:

1. **Register properties** — store details like address, size, year built, estimated value.
2. **Schedule inspections** — send an inspector to visit a property on a specific date.
3. **Score the risk** — after an inspection, calculate how risky the property is (e.g. old roof, flood zone).
4. **Generate reports** — produce a compliance or inspection report that the client can download.
5. **Send notifications** — alert the property owner, inspector, or client that something happened.

This project is that software — built as a **microservices platform**, meaning each of those five jobs is handled by a separate, independent program.

---

## 2. What Is a Microservice?

### The old way — a Monolith

In a traditional application (called a *monolith*), everything lives in one big program. Login, property management, inspections, reports — all in one `.exe` or one deployed website. This is simple to start but becomes painful as the project grows:

- If the report generator crashes, the entire system goes down.
- You cannot scale just the busy parts independently.
- Teams step on each other's code.

### The new way — Microservices

Split the application into **small, independent services**, each:

- Running as its own process (its own program / executable).
- Having its own database.
- Communicating with others via HTTP or messages (like email between departments).

```
┌──────────────────────────────────────────────────────────┐
│                       MONOLITH                           │
│   [Login] [Properties] [Inspections] [Reports] [Notify]  │
└──────────────────────────────────────────────────────────┘

          vs.

[PropertyService] ──► [InspectionService] ──► [RiskScoringService]
                                    │
                           [DocumentService]
                                    │
                        [NotificationService]
```

**Advantage:** If RiskScoringService crashes, properties and inspections still work. You can restart just that one service.

---

## 3. The Five Services

### 3.1 PropertyService (C# / ASP.NET Core 10)

**Job:** The master record of all properties.

- **Port:** 5001
- **Database:** `property.db` (SQLite file)
- **Language:** C#
- **Framework:** ASP.NET Core 10 (traditional controller-based API)

**What it stores:**
```
Property
├── Id (unique ID, e.g. "11111111-1111-1111-1111-111111111111")
├── PropertyReference (human-readable, e.g. "PROP-20260620-AB12CD34")
├── Address, City, State, PostalCode, Country
├── Type (Residential / Commercial / Industrial)
├── YearBuilt, SquareFootage, Bedrooms, Bathrooms
├── EstimatedValue
├── OwnerId (who owns this property)
└── Status (Available / Sold / UnderInspection / etc.)
```

**API endpoints:**
```
GET    /api/properties              → list all properties
GET    /api/properties/{id}         → get one property by ID
POST   /api/properties              → create a new property
PUT    /api/properties/{id}         → update a property
DELETE /api/properties/{id}         → delete a property
GET    /api/properties/owner/{id}   → get properties by owner
```

**Seed data (auto-loaded on first start):**
- 123 Main Street, New York, NY (Residential, $650,000)
- 456 Oak Avenue, Los Angeles, CA (Commercial, $2,100,000)
- 789 Pine Road, Chicago, IL (Residential, $425,000)

---

### 3.2 InspectionService (C# / ASP.NET Core 10 — Minimal API)

**Job:** Schedule and manage property inspections.

- **Port:** 5002
- **Database:** `inspection.db` (SQLite file)
- **Language:** C#
- **Framework:** ASP.NET Core 10 **Minimal API** (a newer, lighter style — see Section 8)

**What it stores:**
```
Inspection
├── Id
├── PropertyId (links to a property in PropertyService)
├── PropertyReference (e.g. "PROP-20260101-DEMO0001")
├── InspectorId, InspectorName
├── Type (Routine / PrePurchase / Annual / Insurance / Compliance / Emergency)
├── Status (Scheduled / InProgress / Completed / Cancelled / Failed)
├── Priority (Low / Medium / High / Critical)
├── ScheduledDate, CompletedDate
├── Notes, Findings
└── OverallConditionScore (0-100)
```

**API endpoints:**
```
GET    /api/inspections                     → list all inspections (page, pageSize)
GET    /api/inspections/{id}                → get one inspection
GET    /api/inspections/property/{propId}   → get inspections for a property
POST   /api/inspections                     → schedule a new inspection
POST   /api/inspections/{id}/complete       → mark an inspection as completed
```

**Seed data:**
- Completed inspection for 123 Main Street (by John Smith, score: 82/100)
- Scheduled pre-purchase inspection for 789 Pine Road (by Sarah Johnson, June 25 2026)

---

### 3.3 DocumentService (C# / ASP.NET Core 10)

**Job:** Generate compliance and inspection reports as downloadable HTML documents.

- **Port:** 5003
- **Database:** `document.db` (SQLite file)
- **Language:** C#
- **Framework:** ASP.NET Core 10 (controller-based)

In production this would generate PDFs stored in Azure Blob Storage. In demo mode it generates **HTML reports** stored directly in the database (no external storage needed).

**API endpoints:**
```
POST   /api/documents/compliance-report           → generate a compliance report
POST   /api/documents/generate-inspection-report  → generate an inspection report
GET    /api/documents/property/{propertyId}        → list documents for a property
GET    /api/documents/{id}                         → get document metadata
GET    /api/documents/{id}/download                → download the HTML report
```

**What a generated report looks like:**
The service builds an HTML page using C# `StringBuilder`, with a styled table showing property details, inspection scores (Foundation, Roof, Electrical, Plumbing, HVAC), compliance status, and recommendations. This HTML is saved to the database and served back when the user clicks "Download".

---

### 3.4 RiskScoringService (Python / FastAPI)

**Job:** Calculate a risk score (0–100) for a property based on inspection data.

- **Port:** 5004
- **Storage:** In-memory list (demo) / Azure Cosmos DB (production)
- **Language:** Python 3.11
- **Framework:** FastAPI

The scoring engine uses **weighted factors:**

| Factor               | Weight | What It Measures |
|----------------------|--------|------------------|
| Structural Condition | 30%    | Inspector's score on building structure |
| Location Risk        | 20%    | Flood zone, crime rate, proximity to hazards |
| Property Age         | 15%    | Newer properties score higher |
| Previous Damage      | 15%    | Any historical damage claims |
| Maintenance History  | 10%    | How well-maintained the property has been |
| Property Type        | 10%    | Commercial properties score slightly lower |

**Risk categories:**
- 80–100 → **Low Risk** (green)
- 60–79  → **Medium Risk** (yellow)
- 40–59  → **High Risk** (orange)
- 0–39   → **Critical Risk** (red)

**API endpoints:**
```
POST   /api/risk/score                      → calculate a risk score
GET    /api/risk/scores                     → list all calculated scores
GET    /api/risk/scores/{property_id}       → get score for a specific property
POST   /api/risk/process-inspection         → trigger scoring from inspection event
GET    /health                              → health check
GET    /info                                → service info
GET    /docs                                → FastAPI interactive documentation
```

---

### 3.5 NotificationService (Python / FastAPI)

**Job:** Send notifications (email, SMS, in-app) when something happens in the platform.

- **Port:** 5005
- **Storage:** In-memory list (demo) / Azure Service Bus (production)
- **Language:** Python 3.11
- **Framework:** FastAPI

In demo mode, notifications are **logged to the console** instead of actually sending emails/SMS (because no SMTP server is configured). In production, it connects to a real email server or Azure Communication Services.

**Built-in event templates:**

| Event Type | What Gets Sent |
|------------|----------------|
| `PropertyCreated` | "A new property has been registered in the system" |
| `InspectionScheduled` | "An inspection has been scheduled for your property" |
| `InspectionCompleted` | "The inspection for your property has been completed" |
| `RiskScoreCalculated` | "A risk assessment has been completed for your property" |
| `ReportGenerated` | "Your compliance report is ready for download" |

**API endpoints:**
```
POST   /api/notifications             → send a direct notification
POST   /api/notifications/event       → trigger notifications from an event type
GET    /api/notifications             → list all notifications
GET    /api/notifications/{id}        → get one notification
GET    /api/notifications/recipient/{id} → get notifications for a user
GET    /health                        → health check
GET    /docs                          → FastAPI interactive documentation
```

---

## 4. How the Code Is Organised (Clean Architecture)

Each .NET service follows **Clean Architecture** — a way of organising code so that the business logic doesn't depend on databases, HTTP, or any external framework. It has four layers:

```
┌───────────────────────────────────────┐
│            API Layer                  │  ← Controllers / Endpoints (HTTP)
├───────────────────────────────────────┤
│         Application Layer             │  ← Commands, Queries, Handlers (business logic)
├───────────────────────────────────────┤
│          Domain Layer                 │  ← Entities, Enums, Interfaces (pure C# classes)
├───────────────────────────────────────┤
│       Infrastructure Layer            │  ← Database, Event Bus (external systems)
└───────────────────────────────────────┘
```

### Why this structure?

**Rule:** Inner layers know nothing about outer layers.

- The `Domain` layer has no reference to Entity Framework or HTTP.
- The `Application` layer doesn't know if you're using SQLite or SQL Server — it only talks to *interfaces*.
- The `Infrastructure` layer holds the actual SQLite/SQL Server code.
- The `API` layer receives HTTP requests and delegates to the Application layer.

**Practical example in PropertyService:**

```
PropertyService.Domain/
├── Entities/Property.cs          ← the Property class (pure C#)
├── Enums/PropertyType.cs         ← Residential, Commercial, etc.
├── Interfaces/IPropertyRepository.cs  ← contract: "I need GetById, Add, etc."
└── Events/PropertyCreatedEvent.cs     ← event object

PropertyService.Application/
├── Commands/CreatePropertyCommand.cs  ← "please create a property with these fields"
├── Handlers/CreatePropertyHandler.cs  ← does the actual work
└── Interfaces/IEventPublisher.cs      ← contract: "I need to publish events"

PropertyService.Infrastructure/
├── Data/PropertyDbContext.cs     ← Entity Framework database context
├── Repositories/PropertyRepository.cs ← implements IPropertyRepository using EF
└── Services/NoOpEventPublisher.cs     ← implements IEventPublisher (does nothing in demo)

PropertyService.API/
├── Controllers/PropertiesController.cs ← handles HTTP GET/POST/PUT/DELETE
└── Program.cs                          ← wires everything together
```

---

## 5. The CQRS Pattern

**CQRS** stands for **Command Query Responsibility Segregation**.

The idea: separate the operations that *change data* (Commands) from operations that *read data* (Queries).

### Without CQRS

```csharp
public class PropertyService {
    public Property GetProperty(Guid id) { ... }   // read
    public void CreateProperty(Property p) { ... }  // write
    public void UpdateProperty(Property p) { ... }  // write
}
// Everything in one class — gets messy as it grows
```

### With CQRS + MediatR

```csharp
// A "Command" — intent to change something
public class CreatePropertyCommand : IRequest<Property>
{
    public string Address { get; set; }
    public string City { get; set; }
    // ... etc
}

// A "Handler" — executes the command
public class CreatePropertyHandler : IRequestHandler<CreatePropertyCommand, Property>
{
    public async Task<Property> Handle(CreatePropertyCommand request, CancellationToken ct)
    {
        // 1. Build the Property object
        // 2. Save to database
        // 3. Publish a "PropertyCreated" event
        // 4. Return the saved property
    }
}

// The controller just sends the command — it doesn't know HOW it's handled
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreatePropertyCommand command)
{
    var result = await _mediator.Send(command);   // MediatR routes it to the handler
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}
```

### MediatR

**MediatR** is a NuGet package (library) that acts as a post office. You give it a message (Command or Query), it finds the right handler, delivers the message, and returns the response. Your controller never needs to `new` up a handler or know the handler class name.

This project uses MediatR for:
- `CreatePropertyCommand` → `CreatePropertyHandler`
- `GetAllInspectionsQuery` → `GetAllInspectionsHandler`
- `GenerateComplianceReportCommand` → `GenerateComplianceReportHandler`
- ... and many more

---

## 6. How Data Is Stored

### Entity Framework Core (EF Core)

EF Core is an **ORM** (Object-Relational Mapper). Instead of writing raw SQL like:

```sql
INSERT INTO Properties (Id, Address, City) VALUES ('abc', '123 Main St', 'New York')
```

You write C#:

```csharp
var property = new Property { Address = "123 Main St", City = "New York" };
await dbContext.Properties.AddAsync(property);
await dbContext.SaveChangesAsync();
```

EF Core translates your C# into the correct SQL automatically.

### DbContext

Every service has a `DbContext` class — think of it as the *connection* to the database:

```csharp
public class PropertyDbContext : DbContext
{
    public DbSet<Property> Properties => Set<Property>();  // maps to "Properties" table

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure columns, indexes, constraints, and seed data
        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.PropertyReference).IsUnique();

            // Seed data — inserted automatically on first run
            entity.HasData(new Property { Id = ..., Address = "123 Main Street", ... });
        });
    }
}
```

### SQLite vs SQL Server

**SQLite** is a tiny, file-based database — no server needed, the database is just a `.db` file on disk. Perfect for demos and development.

**SQL Server** (Azure SQL) is the production database used in the cloud — handles millions of records, backups, high availability.

This project switches between them based on the connection string in `appsettings.json`:

```csharp
// In Program.cs
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (connectionString.Contains(".db"))          // SQLite path detected
{
    options.UseSqlite(connectionString);       // use SQLite
}
else
{
    options.UseSqlServer(connectionString);    // use SQL Server
}
```

Demo `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=property.db"   // SQLite file
  }
}
```

Production environment variable would look like:
```
ConnectionStrings__DefaultConnection=Server=myserver.database.windows.net;Database=PropertyDb;...
```

---

## 7. How Services Talk to Each Other

### The Problem

PropertyService and NotificationService are separate programs. When a property is created, how does NotificationService know to send a "welcome" notification?

**Option A — Direct HTTP call:** PropertyService calls NotificationService's API directly.
- Problem: If NotificationService is down, the property creation fails. They are *tightly coupled*.

**Option B — Message Queue (Events):** PropertyService drops a message in a queue. NotificationService picks it up whenever it's ready.
- Benefit: PropertyService doesn't wait. If NotificationService is down, the message waits in the queue. *Loosely coupled.*

### Domain Events

When something important happens, the service creates an **event object**:

```csharp
// PropertyService.Domain/Events/PropertyCreatedEvent.cs
public class PropertyCreatedEvent
{
    public Guid PropertyId { get; set; }
    public string PropertyReference { get; set; }
    public string Address { get; set; }
    public string OwnerId { get; set; }
    public decimal EstimatedValue { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### IEventPublisher — The Pluggable Interface

The Application layer uses an *interface* — a contract that says "I need something that can publish events, but I don't care how":

```csharp
public interface IEventPublisher
{
    Task PublishAsync<T>(string topicOrQueue, T eventMessage, CancellationToken ct = default);
}
```

Two implementations exist:

**1. NoOpEventPublisher (demo mode) — does nothing:**
```csharp
public class NoOpEventPublisher : IEventPublisher
{
    public Task PublishAsync<T>(string topicOrQueue, T eventMessage, CancellationToken ct)
    {
        _logger.LogDebug("Event published (no-op): {EventType}", typeof(T).Name);
        return Task.CompletedTask;  // just logs and moves on
    }
}
```

**2. ServiceBusEventPublisher (production) — sends to Azure Service Bus:**
```csharp
public class ServiceBusEventPublisher : IEventPublisher
{
    public async Task PublishAsync<T>(string topicOrQueue, T eventMessage, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(eventMessage);
        var message = new ServiceBusMessage(json);
        var sender = _client.CreateSender(topicOrQueue);
        await sender.SendMessageAsync(message, ct);  // sends to Azure Service Bus
    }
}
```

Which one gets used is decided at startup in `Program.cs`:

```csharp
var serviceBusConnection = builder.Configuration.GetConnectionString("ServiceBus");

if (!string.IsNullOrEmpty(serviceBusConnection))
{
    // Production: real Azure Service Bus
    builder.Services.AddSingleton(new ServiceBusClient(serviceBusConnection));
    builder.Services.AddScoped<IEventPublisher, ServiceBusEventPublisher>();
}
else
{
    // Demo: no-op (just logs)
    builder.Services.AddScoped<IEventPublisher, NoOpEventPublisher>();
}
```

### Azure Service Bus (Production)

Think of Azure Service Bus as a **post office in the cloud**:
- Services drop messages (letters) into **queues** or **topics**.
- Other services subscribe and pick up messages in their own time.
- If a service crashes, messages are not lost — they wait in the queue.

```
PropertyService ──publishes──► [property-events topic] ──► NotificationService subscribes
                                                       ──► RiskScoringService subscribes
```

---

## 8. The API Layer

### Traditional Controller (PropertyService, DocumentService)

```csharp
[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropertiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Sends a query to MediatR, gets back a list
        var result = await _mediator.Send(new GetAllPropertiesQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePropertyCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```

### Minimal API (InspectionService)

ASP.NET Core 10 introduced a lighter syntax — no controller class needed:

```csharp
// In InspectionEndpoints.cs
var group = app.MapGroup("/api/inspections").WithTags("Inspections");

group.MapPost("/", async Task<IResult> (CreateInspectionCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return Results.Created($"/api/inspections/{result.Id}", result);
});

group.MapGet("/", async Task<IResult> (int page = 1, int pageSize = 10, IMediator mediator = default!) =>
{
    var result = await mediator.Send(new GetAllInspectionsQuery { Page = page, PageSize = pageSize });
    return Results.Ok(result);
});
```

Same result, less boilerplate code.

### Swagger (API Documentation)

Every .NET service includes **Swagger UI** — a web page that documents all the API endpoints and lets you test them interactively in the browser. Open any service's root URL to see it:

- http://localhost:5001 → PropertyService Swagger
- http://localhost:5002 → InspectionService Swagger
- http://localhost:5003 → DocumentService Swagger

It's configured in `Program.cs`:

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Property Service API", Version = "v1" });
});

// ...

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Property Service API v1");
    c.RoutePrefix = string.Empty;  // serve at root "/" instead of "/swagger"
});
```

### Dependency Injection (DI)

All the services, repositories, and handlers are wired together in `Program.cs` using ASP.NET Core's built-in **Dependency Injection container**:

```csharp
// "When someone asks for IPropertyRepository, give them a PropertyRepository"
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();

// "When someone asks for IEventPublisher, give them a NoOpEventPublisher"
builder.Services.AddScoped<IEventPublisher, NoOpEventPublisher>();

// "Register all MediatR handlers found in the Application assembly"
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreatePropertyHandler).Assembly);
});
```

**Scoped** means: create one instance per HTTP request, then dispose it.

---

## 9. The Python Services

### Why Python?

The RiskScoringService and NotificationService are written in Python to demonstrate **polyglot microservices** — different services can use different programming languages because they communicate via HTTP, not shared code.

### FastAPI

FastAPI is a modern Python web framework similar to ASP.NET Core. It automatically generates interactive API documentation (at `/docs`).

```python
from fastapi import FastAPI
from pydantic import BaseModel

app = FastAPI(title="Risk Scoring Service")

class RiskScoreRequest(BaseModel):      # Like a C# class with properties
    property_id: str
    inspection_data: dict

@app.post("/api/risk/score")            # Like [HttpPost] in C#
async def calculate_risk(request: RiskScoreRequest):
    engine = RiskScoringEngine()
    result = engine.calculate(request.inspection_data, request.property_details)
    return result
```

### In-Memory Storage (Demo)

Since the Python services don't have a database in demo mode, they use a simple Python list:

```python
risk_scores_store: list[dict] = []  # lives in memory, resets when container restarts

@app.post("/api/risk/score")
async def calculate_risk(request: RiskScoreRequest):
    result = engine.calculate(...)
    risk_scores_store.append(result)   # "save" to memory
    return result

@app.get("/api/risk/scores")
async def list_scores():
    return {"total": len(risk_scores_store), "items": risk_scores_store}
```

In production this would save to **Azure Cosmos DB** — a globally distributed, schema-free database hosted in Azure.

---

## 10. Docker

### What Is Docker?

Docker packages an application and *everything it needs* (runtime, libraries, configuration) into a **container** — a lightweight, portable box that runs identically on any computer.

Without Docker: "It works on my machine but not on the server."
With Docker: The container *is* the machine — same everywhere.

### Dockerfile

Each service has a `Dockerfile` that describes how to build its container image:

```dockerfile
# Start from the official .NET 10 SDK image (has everything to compile code)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore NuGet packages
COPY ["PropertyService.API/PropertyService.API.csproj", "PropertyService.API/"]
RUN dotnet restore "PropertyService.API/PropertyService.API.csproj"

# Copy all source code and build
COPY . .
RUN dotnet publish "PropertyService.API.csproj" -c Release -o /app/publish

# Now create a smaller final image — only the runtime, not the SDK
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PropertyService.API.dll"]   # command that starts the app
```

### Docker Compose

`docker-compose.yml` is the file that starts **all six containers** together with a single command:

```yaml
services:
  property-service:
    build: ./src/PropertyService        # build using that service's Dockerfile
    ports:
      - "5001:8080"                     # map host port 5001 to container port 8080
    environment:
      - ASPNETCORE_URLS=http://+:8080   # tell .NET to listen on port 8080 inside container
      - ConnectionStrings__DefaultConnection=Data Source=/data/property.db
    volumes:
      - service-data:/data              # shared folder so the .db file persists between restarts

  risk-scoring-service:
    build: ./src/RiskScoringService
    ports:
      - "5004:8000"                     # FastAPI runs on 8000 inside the container

  demo-dashboard:
    image: nginx:alpine                 # use the official nginx image
    ports:
      - "8080:80"
    volumes:
      - ./demo/dashboard:/usr/share/nginx/html:ro   # serve the dashboard HTML
```

**Running the whole platform:**
```bash
docker compose up -d      # -d means run in background (detached)
docker compose down       # stop and remove all containers
docker compose logs -f    # stream logs from all containers
```

### Port Mapping

Each service runs on port 8080 *inside* its container, but is accessible from the outside on different ports:

```
Your Browser     Container
localhost:5001 → property-service:8080   (PropertyService)
localhost:5002 → inspection-service:8080 (InspectionService)
localhost:5003 → document-service:8080   (DocumentService)
localhost:5004 → risk-scoring:8000       (RiskScoringService)
localhost:5005 → notification:8000       (NotificationService)
localhost:8080 → nginx:80                (Demo Dashboard)
```

---

## 11. How Azure Fits In

This project is designed to run locally for demos, but is **Azure-ready** — flip a few configuration values and it uses real cloud services.

### Azure Kubernetes Service (AKS)
Instead of `docker compose up` on one machine, in production each container runs on a **Kubernetes cluster** — a group of virtual machines managed by Azure. Kubernetes handles:
- Restarting crashed containers automatically.
- Scaling up when traffic is high (run 10 copies of PropertyService).
- Rolling deployments with zero downtime.

### Azure SQL Database
Replaces SQLite `.db` files. A fully managed relational database hosted in Azure with automatic backups, high availability, and geo-replication.

Connection string swap (environment variable in production):
```
ConnectionStrings__DefaultConnection=Server=mydb.database.windows.net;Database=PropertyDb;Authentication=Active Directory Managed Identity
```

### Azure Cosmos DB
Replaces the in-memory lists in the Python services. A globally distributed, NoSQL database. The RiskScoringService would store risk scores here.

### Azure Service Bus
Replaces the NoOpEventPublisher. A fully managed message queue. When PropertyService publishes a `PropertyCreatedEvent`, it goes into a Service Bus topic. NotificationService and RiskScoringService subscribe and react.

### Azure Blob Storage
In production, DocumentService would upload generated HTML/PDF files to Blob Storage and store only a URL in the database. This allows files to be stored cheaply and served globally via Azure CDN.

### Azure Key Vault
Stores secrets (database passwords, API keys, connection strings) securely. Services use **Managed Identity** — no password needed in code; Azure grants access automatically.

### Summary

| Demo Mode | Production (Azure) |
|-----------|-------------------|
| SQLite `.db` file | Azure SQL Database |
| In-memory Python list | Azure Cosmos DB |
| NoOpEventPublisher | Azure Service Bus |
| HTML stored in DB | Azure Blob Storage |
| `docker compose up` | Azure Kubernetes Service |
| Hardcoded config | Azure App Configuration + Key Vault |

---

## 12. The Full Operational Flow — Step by Step

Here is what happens when a user goes through the complete platform workflow:

### Step 1 — Register a Property

**User action:** POST to `http://localhost:5001/api/properties` with JSON body:
```json
{
  "address": "500 Demo Avenue",
  "city": "Seattle",
  "state": "WA",
  "postalCode": "98101",
  "country": "USA",
  "type": 0,
  "yearBuilt": 2015,
  "squareFootage": 2200,
  "estimatedValue": 680000,
  "ownerId": "user-42",
  "bedrooms": 4,
  "bathrooms": 3
}
```

**What happens inside PropertyService:**
1. ASP.NET Core receives the HTTP POST request.
2. The JSON body is automatically deserialized into a `CreatePropertyCommand` object.
3. The controller calls `_mediator.Send(command)`.
4. MediatR routes it to `CreatePropertyHandler.Handle(...)`.
5. The handler creates a `Property` entity, sets `Id = Guid.NewGuid()`, generates `PropertyReference = "PROP-20260620-XXXXXXXX"`.
6. Calls `_repository.AddAsync(property)` → EF Core queues an INSERT.
7. Calls `SaveChangesAsync()` → EF Core runs the SQL INSERT on the SQLite database.
8. Creates a `PropertyCreatedEvent` object.
9. Calls `_eventPublisher.PublishAsync("property-events", evt)` → In demo mode, this just logs a line. In production, it sends a JSON message to Azure Service Bus.
10. Returns the saved `Property` object.
11. Controller returns `HTTP 201 Created` with the property JSON.

**Response:**
```json
{
  "id": "2eb92053-e452-4182-8eb5-a5f496f4b1be",
  "propertyReference": "PROP-20260620-2AA32132",
  "address": "500 Demo Avenue",
  "city": "Seattle",
  ...
}
```

---

### Step 2 — Schedule an Inspection

**User action:** POST to `http://localhost:5002/api/inspections`:
```json
{
  "propertyId": "2eb92053-e452-4182-8eb5-a5f496f4b1be",
  "propertyReference": "PROP-20260620-2AA32132",
  "inspectorId": "insp-001",
  "inspectorName": "John Smith",
  "type": 0,
  "priority": 1,
  "scheduledDate": "2026-08-01T10:00:00",
  "notes": "Annual routine inspection"
}
```

**What happens inside InspectionService:**
1. The Minimal API endpoint receives the POST request.
2. MediatR routes to `CreateInspectionHandler`.
3. An `Inspection` entity is created with `Status = Scheduled`.
4. Saved to `inspection.db` via EF Core.
5. Returns `HTTP 201 Created` with the inspection JSON.

---

### Step 3 — Calculate Risk Score

**User action:** POST to `http://localhost:5004/api/risk/score`:
```json
{
  "property_id": "2eb92053-e452-4182-8eb5-a5f496f4b1be",
  "inspection_id": "d3c319d9-0fa9-4733-a618-94d436eae0fc",
  "inspection_data": {
    "structural_condition": { "score": 88 },
    "previous_damage": { "score": 92 },
    "maintenance_history": { "score": 85 },
    "location": {}
  },
  "property_details": {
    "year_built": 2015,
    "type": "residential"
  }
}
```

**What happens inside RiskScoringService (Python):**
1. FastAPI receives the POST request.
2. Pydantic validates the JSON into a `RiskScoreRequest` object.
3. `RiskScoringEngine.calculate()` runs the weighted formula:
   - Structural: 88 × 0.30 = 26.4
   - Age: (100 - 11×1.5) = 83.5 × 0.15 = 12.5
   - Location: 75 × 0.20 = 15.0 (default, no flood zone data)
   - Type: 85 × 0.10 = 8.5 (residential)
   - Damage: 92 × 0.15 = 13.8
   - Maintenance: 85 × 0.10 = 8.5
   - **Total: 84.7 → "Low Risk"**
4. Result is appended to `risk_scores_store` (in-memory list).
5. Returns the score with breakdown and recommendations.

**Response:**
```json
{
  "property_id": "2eb92053-...",
  "risk_score": 84.73,
  "risk_category": "Low Risk",
  "factors": [ ... ],
  "recommendations": ["Property appears in good condition — maintain regular inspection schedule"]
}
```

---

### Step 4 — Generate a Report

**User action:** POST to `http://localhost:5003/api/documents/compliance-report`:
```json
{
  "propertyId": "2eb92053-e452-4182-8eb5-a5f496f4b1be",
  "reportType": "Compliance"
}
```

**What happens inside DocumentService:**
1. Controller receives the POST.
2. MediatR routes to `GenerateComplianceReportHandler`.
3. The handler builds an HTML string using `StringBuilder`:
   ```csharp
   sb.AppendLine("<html><head><title>Compliance Report</title></head>");
   sb.AppendLine("<body><h1>Property Compliance Report</h1>");
   sb.AppendLine($"<p>Property ID: {request.PropertyId}</p>");
   sb.AppendLine("<table><tr><th>Item</th><th>Status</th></tr>");
   sb.AppendLine("<tr><td>Fire Safety</td><td>Compliant</td></tr>");
   // ... etc
   sb.AppendLine("</body></html>");
   ```
4. A `Document` entity is created with `ContentText = htmlString`, `Status = Completed`.
5. Saved to `document.db`.
6. Returns the document metadata.

**Download:** `GET http://localhost:5003/api/documents/{id}/download` returns the raw HTML content with `Content-Type: text/html`.

---

### Step 5 — Send a Notification

**User action:** POST to `http://localhost:5005/api/notifications/event`:
```json
{
  "event_type": "ReportGenerated",
  "payload": { "property_id": "2eb92053-..." },
  "channels": ["in_app", "email"]
}
```

**What happens inside NotificationService (Python):**
1. FastAPI receives the POST.
2. The handler looks up the template for `ReportGenerated`.
3. Creates notification records in `notifications_store`.
4. In demo mode: logs "Sending notification: Your compliance report is ready."
5. In production: would call SMTP to send a real email.
6. Returns `{ "status": "accepted", "notification_id": "evt-..." }`.

---

## 13. Configuration

### How .NET reads configuration

ASP.NET Core reads configuration from multiple sources in order (later ones override earlier):
1. `appsettings.json` (base config, committed to git)
2. `appsettings.Development.json` (dev overrides, not committed)
3. Environment variables (set by Docker Compose or Kubernetes)

**appsettings.json (demo defaults):**
```json
{
  "Logging": { "LogLevel": { "Default": "Information" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=property.db"
  }
}
```

**Docker Compose overrides via environment variables:**
```yaml
environment:
  - ConnectionStrings__DefaultConnection=Data Source=/data/property.db
  - ASPNETCORE_ENVIRONMENT=Development
```

Note: `ConnectionStrings__DefaultConnection` — double underscore (`__`) is how ASP.NET Core reads nested JSON keys from environment variables.

### How Python reads configuration

```python
class Settings:
    def __init__(self):
        # os.getenv("VAR", "default") → reads from environment, falls back to default
        self.cosmos_db_connection = os.getenv("COSMOS_DB_CONNECTION", "")
        self.smtp_host = os.getenv("SMTP_HOST", "smtp.gmail.com")
```

If `COSMOS_DB_CONNECTION` is not set → empty string → use in-memory store.
If it is set → connect to Azure Cosmos DB.

---

## 14. The Demo Dashboard

`demo/dashboard/index.html` is a single HTML file with embedded JavaScript that calls all 5 service APIs directly from the browser.

### How it works

```javascript
// Check health of all services on page load
const SERVICES = [
  { name: "Property Service",    port: 5001, path: "/health" },
  { name: "Inspection Service",  port: 5002, path: "/health" },
  { name: "Document Service",    port: 5003, path: "/health" },
  { name: "Risk Scoring",        port: 5004, path: "/health" },
  { name: "Notification Service",port: 5005, path: "/health" },
];

// Fetch data from PropertyService
async function loadProperties() {
  const r = await fetch("http://localhost:5001/api/properties");
  const data = await r.json();
  // render as HTML table
}

// Submit the "Create Property" form
async function createProperty() {
  const body = { address: ..., city: ..., ... };  // collected from form inputs
  const r = await fetch("http://localhost:5001/api/properties", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body)
  });
}
```

### Why CORS is enabled

CORS (Cross-Origin Resource Sharing) is a browser security rule. When `index.html` running at `http://localhost:8080` tries to call `http://localhost:5001/api/properties`, the browser blocks the request because the ports differ (different "origins").

Each .NET service enables CORS to allow this:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

app.UseCors("AllowAll");
```

---

## 15. Observability

### Health Checks

Every service exposes `GET /health` — a simple endpoint that returns `"Healthy"` if everything is working (database reachable, etc.):

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PropertyDbContext>();   // checks if the DB is reachable

app.MapHealthChecks("/health");
```

Docker Compose uses this to know if a container is ready:
```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
  interval: 30s
  retries: 3
```

### OpenTelemetry (Distributed Tracing)

When a user creates a property, that request might touch multiple services. OpenTelemetry attaches a **trace ID** to every request — a unique identifier that links related operations across all services, making it easy to debug what happened where.

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("PropertyService"))
        .AddAspNetCoreInstrumentation()     // trace incoming HTTP requests
        .AddHttpClientInstrumentation());   // trace outgoing HTTP calls
```

In production, traces are sent to **Azure Monitor / Application Insights** where you can visualise the full call chain.

---

## 16. Glossary

| Term | Plain English Explanation |
|------|--------------------------|
| **API** | A web address your code calls to get or send data. Like a waiter — you order, they bring food. |
| **ASP.NET Core** | Microsoft's framework for building web APIs in C#. |
| **Azure** | Microsoft's cloud platform — rent servers, databases, and services instead of buying hardware. |
| **Clean Architecture** | An organising pattern where business logic (Domain/Application) is isolated from frameworks (Infrastructure/API). |
| **Container** | A packaged app that runs identically anywhere (via Docker). Like a shipping container — same box, different ship. |
| **CORS** | Browser security rule preventing one website from calling another. Services must explicitly allow it. |
| **CQRS** | Separating "write" operations (Commands) from "read" operations (Queries). |
| **DbContext** | EF Core's connection to the database. Your code's gateway to running SQL without writing SQL. |
| **Dependency Injection** | A system where objects are given their dependencies instead of creating them. Enables swapping implementations (e.g., SQLite ↔ SQL Server). |
| **Docker** | Tool for packaging apps into containers. `docker compose up` starts everything. |
| **Domain Event** | A signal that something important happened (e.g., `PropertyCreatedEvent`). Other services react to it. |
| **EF Core** | Entity Framework Core — lets you work with databases using C# objects instead of SQL strings. |
| **FastAPI** | Python web framework for building APIs. Automatically generates interactive docs at `/docs`. |
| **Health Check** | `GET /health` endpoint that returns whether a service is running correctly. |
| **IEventPublisher** | An interface (contract) for publishing events. Swapped between NoOp (demo) and Azure Service Bus (production). |
| **Kubernetes (AKS)** | System that runs and manages multiple containers across many servers. Azure AKS is the managed version. |
| **MediatR** | Library that routes Commands and Queries to the right Handler. Like a dispatcher. |
| **Microservice** | A small, independent program with a single responsibility and its own database. |
| **Minimal API** | Lighter ASP.NET Core style — define endpoints inline in `Program.cs` without controller classes. |
| **Monolith** | One big application with everything in it. Simpler to start, harder to scale. |
| **NoOpEventPublisher** | An event publisher that does nothing (just logs). Used in demo mode. |
| **OpenTelemetry** | Standard for collecting traces, metrics, and logs across services. |
| **ORM** | Object-Relational Mapper — translates C# objects to database rows. EF Core is an ORM. |
| **Port** | A numbered "door" on a computer where a service listens for connections. |
| **Pydantic** | Python library for data validation — like C# data annotations but for Python classes. |
| **Repository Pattern** | A layer that abstracts database calls. `IPropertyRepository.GetById(id)` vs raw SQL. |
| **Scoped** | DI lifetime — one object per HTTP request. |
| **Seed Data** | Test data automatically inserted into the database on first startup. |
| **Service Bus** | Azure's managed message queue. Services drop messages in; other services pick them up. |
| **SQLite** | Tiny file-based database. No server needed — the data is just a `.db` file. |
| **Swagger** | Auto-generated documentation UI for APIs. Test endpoints directly in the browser. |
| **Unit of Work** | A pattern that groups database changes into a single transaction. |
| **Weighted Score** | A formula where different factors count for different percentages of the total. |
