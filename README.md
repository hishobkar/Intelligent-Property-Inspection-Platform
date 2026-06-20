# Intelligent Property Inspection Platform

An enterprise-grade, event-driven microservices solution for property inspection and risk scoring.

## Quick Start (Demo Mode — no Azure required)

Runs fully locally with SQLite and in-memory stores. Only Docker Desktop is required.

```bash
# Clone and start everything
git clone https://github.com/your-repo/intelligent-property-inspection.git
cd intelligent-property-inspection
docker compose up -d
```

Then open the demo dashboard in your browser:
- **Demo Dashboard**: http://localhost:8080
- **Property Service** (Swagger): http://localhost:5001
- **Inspection Service** (Swagger): http://localhost:5002
- **Document Service** (Swagger): http://localhost:5003
- **Risk Scoring Service** (FastAPI Docs): http://localhost:5004/docs
- **Notification Service** (FastAPI Docs): http://localhost:5005/docs

To stop: `docker compose down`

> **Alternative** (Windows, without Docker for .NET services):
> ```powershell
> .\scripts\start-demo.ps1
> ```
> Requires .NET 10 SDK + Docker Desktop.

---

## Architecture Overview

The platform consists of five microservices:

1. **Property Service** (ASP.NET Core 10) - Manages property records and publishes events
2. **Inspection Service** (ASP.NET Core 10 Minimal API) - Handles property inspections
3. **Document Service** (ASP.NET Core 10) - Generates compliance reports (HTML)
4. **Risk Scoring Service** (Python FastAPI) - Calculates risk scores based on inspection data
5. **Notification Service** (Python FastAPI) - Sends notifications based on events

## Key Features

- **Demo Mode**: Runs fully locally without any cloud dependencies (SQLite + in-memory stores)
- **Event-Driven Architecture**: Services publish domain events; pluggable event bus (NoOp in demo, Azure Service Bus in production)
- **CQRS Pattern**: Separate commands and queries via MediatR for optimal performance
- **Clean Architecture**: Domain-centric design with clear separation of concerns (Domain / Application / Infrastructure / API)
- **OpenTelemetry**: Distributed tracing and metrics collection
- **Azure-Ready**: Configurable for Azure Service Bus, Cosmos DB, SQL Server, and Blob Storage via environment variables
- **Observability**: Health check endpoints on all services, structured logging, and distributed tracing
- **Infrastructure as Code**: Terraform for Azure resources, Helm charts for Kubernetes

## Technology Stack

### Backend Services
- .NET 10 (Property, Inspection, Document services)
- Python 3.11 (Risk Scoring, Notification services)
- ASP.NET Core 10 Web API & Minimal API
- FastAPI (Python)
- Entity Framework Core
- MediatR (CQRS)
- OpenTelemetry

### Infrastructure
- Azure Kubernetes Service (AKS)
- Azure Container Registry (ACR)
- Azure API Management
- Azure Service Bus
- Azure Event Grid
- Azure SQL Database
- Azure Cosmos DB
- Azure Blob Storage
- Azure Key Vault
- Azure App Configuration
- Application Insights
- Azure Monitor

### DevOps
- Terraform (IaC)
- Helm Charts
- GitHub Actions (CI/CD)
- Docker

## Getting Started

### Prerequisites

**Demo mode (no Azure):**
- Docker Desktop

**Full production mode:**
- .NET 10 SDK
- Python 3.11+
- Docker Desktop
- Azure CLI, Kubectl, Helm, Terraform

### Local Demo (Docker Compose)

```bash
# Start all services
docker compose up -d

# View logs
docker compose logs -f

# Stop all services
docker compose down
```

Seed data is loaded automatically: 3 demo properties and 2 inspections are available on first start.

### Local Development (.NET services without Docker)

1. Clone the repository:
```bash
git clone https://github.com/your-repo/intelligent-property-inspection.git
cd intelligent-property-inspection
```


### Project Structure

```
intelligent-property-inspection/
├── .github/
│   └── workflows/
│       ├── ci-cd.yml
│       └── security-scan.yml
├── infrastructure/
│   ├── terraform/
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   ├── outputs.tf
│   │   ├── provider.tf
│   │   ├── terraform.tfvars.example
│   │   ├── modules/
│   │   │   ├── aks/
│   │   │   │   ├── main.tf
│   │   │   │   ├── variables.tf
│   │   │   │   └── outputs.tf
│   │   │   ├── database/
│   │   │   │   ├── main.tf
│   │   │   │   ├── variables.tf
│   │   │   │   └── outputs.tf
│   │   │   ├── messaging/
│   │   │   │   ├── main.tf
│   │   │   │   ├── variables.tf
│   │   │   │   └── outputs.tf
│   │   │   └── monitoring/
│   │   │       ├── main.tf
│   │   │       ├── variables.tf
│   │   │       └── outputs.tf
│   │   └── environments/
│   │       ├── dev.tfvars
│   │       ├── staging.tfvars
│   │       └── prod.tfvars
│   └── kubernetes/
│       ├── base/
│       │   ├── namespace.yaml
│       │   ├── ingress.yaml
│       │   ├── secrets.yaml
│       │   ├── configmap.yaml
│       │   ├── service-account.yaml
│       │   └── cluster-role.yaml
│       └── overlays/
│           ├── dev/
│           │   ├── kustomization.yaml
│           │   ├── ingress-patch.yaml
│           │   └── configmap-patch.yaml
│           ├── staging/
│           │   ├── kustomization.yaml
│           │   ├── ingress-patch.yaml
│           │   └── configmap-patch.yaml
│           └── prod/
│               ├── kustomization.yaml
│               ├── ingress-patch.yaml
│               └── configmap-patch.yaml
├── charts/
│   ├── property-service/
│   │   ├── Chart.yaml
│   │   ├── values.yaml
│   │   ├── values-dev.yaml
│   │   ├── values-prod.yaml
│   │   ├── templates/
│   │   │   ├── _helpers.tpl
│   │   │   ├── deployment.yaml
│   │   │   ├── service.yaml
│   │   │   ├── ingress.yaml
│   │   │   ├── configmap.yaml
│   │   │   ├── secret.yaml
│   │   │   ├── serviceaccount.yaml
│   │   │   ├── hpa.yaml
│   │   │   ├── pdb.yaml
│   │   │   ├── servicemonitor.yaml
│   │   │   └── tests/
│   │   │       └── test-connection.yaml
│   │   └── charts/
│   ├── inspection-service/
│   │   ├── Chart.yaml
│   │   ├── values.yaml
│   │   ├── values-dev.yaml
│   │   ├── values-prod.yaml
│   │   └── templates/
│   │       ├── _helpers.tpl
│   │       ├── deployment.yaml
│   │       ├── service.yaml
│   │       ├── ingress.yaml
│   │       ├── configmap.yaml
│   │       ├── secret.yaml
│   │       ├── serviceaccount.yaml
│   │       ├── hpa.yaml
│   │       ├── pdb.yaml
│   │       ├── servicemonitor.yaml
│   │       └── tests/
│   │           └── test-connection.yaml
│   ├── risk-scoring-service/
│   │   ├── Chart.yaml
│   │   ├── values.yaml
│   │   ├── values-dev.yaml
│   │   ├── values-prod.yaml
│   │   └── templates/
│   │       ├── _helpers.tpl
│   │       ├── deployment.yaml
│   │       ├── service.yaml
│   │       ├── ingress.yaml
│   │       ├── configmap.yaml
│   │       ├── secret.yaml
│   │       ├── serviceaccount.yaml
│   │       ├── hpa.yaml
│   │       ├── pdb.yaml
│   │       ├── servicemonitor.yaml
│   │       └── tests/
│   │           └── test-connection.yaml
│   ├── document-service/
│   │   ├── Chart.yaml
│   │   ├── values.yaml
│   │   ├── values-dev.yaml
│   │   ├── values-prod.yaml
│   │   └── templates/
│   │       ├── _helpers.tpl
│   │       ├── deployment.yaml
│   │       ├── service.yaml
│   │       ├── ingress.yaml
│   │       ├── configmap.yaml
│   │       ├── secret.yaml
│   │       ├── serviceaccount.yaml
│   │       ├── hpa.yaml
│   │       ├── pdb.yaml
│   │       ├── servicemonitor.yaml
│   │       └── tests/
│   │           └── test-connection.yaml
│   └── notification-service/
│       ├── Chart.yaml
│       ├── values.yaml
│       ├── values-dev.yaml
│       ├── values-prod.yaml
│       └── templates/
│           ├── _helpers.tpl
│           ├── deployment.yaml
│           ├── service.yaml
│           ├── ingress.yaml
│           ├── configmap.yaml
│           ├── secret.yaml
│           ├── serviceaccount.yaml
│           ├── hpa.yaml
│           ├── pdb.yaml
│           ├── servicemonitor.yaml
│           └── tests/
│               └── test-connection.yaml
├── src/
│   ├── PropertyService/
│   │   ├── PropertyService.API/
│   │   │   ├── Controllers/
│   │   │   │   ├── PropertiesController.cs
│   │   │   │   ├── HealthController.cs
│   │   │   │   └── MetricsController.cs
│   │   │   ├── Middleware/
│   │   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   │   ├── RequestLoggingMiddleware.cs
│   │   │   │   └── CorrelationIdMiddleware.cs
│   │   │   ├── Filters/
│   │   │   │   ├── ValidationFilter.cs
│   │   │   │   └── AuthorizeFilter.cs
│   │   │   ├── Validators/
│   │   │   │   ├── CreatePropertyValidator.cs
│   │   │   │   └── UpdatePropertyValidator.cs
│   │   │   ├── Program.cs
│   │   │   ├── Startup.cs
│   │   │   ├── appsettings.json
│   │   │   ├── appsettings.Development.json
│   │   │   ├── appsettings.Production.json
│   │   │   ├── appsettings.Docker.json
│   │   │   ├── PropertyService.API.csproj
│   │   │   └── Properties/
│   │   │       └── launchSettings.json
│   │   ├── PropertyService.Application/
│   │   │   ├── Commands/
│   │   │   │   ├── CreatePropertyCommand.cs
│   │   │   │   ├── UpdatePropertyCommand.cs
│   │   │   │   ├── DeletePropertyCommand.cs
│   │   │   │   └── ProcessPropertyEventCommand.cs
│   │   │   ├── Handlers/
│   │   │   │   ├── CreatePropertyHandler.cs
│   │   │   │   ├── UpdatePropertyHandler.cs
│   │   │   │   ├── DeletePropertyHandler.cs
│   │   │   │   └── ProcessPropertyEventHandler.cs
│   │   │   ├── Queries/
│   │   │   │   ├── GetPropertyByIdQuery.cs
│   │   │   │   ├── GetAllPropertiesQuery.cs
│   │   │   │   ├── GetPropertiesByOwnerQuery.cs
│   │   │   │   └── SearchPropertiesQuery.cs
│   │   │   ├── DTOs/
│   │   │   │   ├── PropertyDto.cs
│   │   │   │   ├── CreatePropertyDto.cs
│   │   │   │   ├── UpdatePropertyDto.cs
│   │   │   │   └── PropertySearchDto.cs
│   │   │   ├── Mappings/
│   │   │   │   └── AutoMapperProfile.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── IPropertyService.cs
│   │   │   │   └── IEventPublisher.cs
│   │   │   ├── Services/
│   │   │   │   ├── PropertyService.cs
│   │   │   │   └── EventPublisherService.cs
│   │   │   ├── Exceptions/
│   │   │   │   ├── PropertyNotFoundException.cs
│   │   │   │   ├── DuplicatePropertyException.cs
│   │   │   │   └── ValidationException.cs
│   │   │   ├── Constants/
│   │   │   │   └── ErrorMessages.cs
│   │   │   ├── PropertyService.Application.csproj
│   │   │   └── DependencyInjection.cs
│   │   ├── PropertyService.Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── Property.cs
│   │   │   │   ├── PropertyHistory.cs
│   │   │   │   └── PropertyOwner.cs
│   │   │   ├── Enums/
│   │   │   │   ├── PropertyType.cs
│   │   │   │   ├── PropertyStatus.cs
│   │   │   │   └── PropertyEventType.cs
│   │   │   ├── Events/
│   │   │   │   ├── PropertyCreatedEvent.cs
│   │   │   │   ├── PropertyUpdatedEvent.cs
│   │   │   │   └── PropertyDeletedEvent.cs
│   │   │   ├── ValueObjects/
│   │   │   │   ├── Address.cs
│   │   │   │   └── Money.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── IPropertyRepository.cs
│   │   │   │   ├── IUnitOfWork.cs
│   │   │   │   └── IAuditable.cs
│   │   │   ├── Specifications/
│   │   │   │   ├── PropertySpecification.cs
│   │   │   │   └── PropertySearchSpecification.cs
│   │   │   ├── PropertyService.Domain.csproj
│   │   │   └── AssemblyInfo.cs
│   │   ├── PropertyService.Infrastructure/
│   │   │   ├── Data/
│   │   │   │   ├── PropertyDbContext.cs
│   │   │   │   ├── DesignTimeDbContextFactory.cs
│   │   │   │   ├── Configurations/
│   │   │   │   │   ├── PropertyConfiguration.cs
│   │   │   │   │   └── PropertyHistoryConfiguration.cs
│   │   │   │   ├── Migrations/
│   │   │   │   │   ├── 20240101000000_InitialCreate.cs
│   │   │   │   │   ├── 20240101000000_InitialCreate.Designer.cs
│   │   │   │   │   ├── 20240115000000_AddPropertyHistory.cs
│   │   │   │   │   ├── 20240115000000_AddPropertyHistory.Designer.cs
│   │   │   │   │   └── PropertyDbContextModelSnapshot.cs
│   │   │   │   └── SeedData/
│   │   │   │       └── PropertySeedData.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── PropertyRepository.cs
│   │   │   │   ├── UnitOfWork.cs
│   │   │   │   └── GenericRepository.cs
│   │   │   ├── Services/
│   │   │   │   ├── ServiceBusPublisher.cs
│   │   │   │   ├── EventGridPublisher.cs
│   │   │   │   └── BlobStorageService.cs
│   │   │   ├── Extensions/
│   │   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   │   └── ConfigurationExtensions.cs
│   │   │   ├── Options/
│   │   │   │   ├── ServiceBusOptions.cs
│   │   │   │   └── StorageOptions.cs
│   │   │   ├── PropertyService.Infrastructure.csproj
│   │   │   └── AssemblyInfo.cs
│   │   ├── PropertyService.Tests/
│   │   │   ├── UnitTests/
│   │   │   │   ├── Application/
│   │   │   │   │   ├── Handlers/
│   │   │   │   │   │   ├── CreatePropertyHandlerTests.cs
│   │   │   │   │   │   └── UpdatePropertyHandlerTests.cs
│   │   │   │   │   └── Services/
│   │   │   │   │       └── PropertyServiceTests.cs
│   │   │   │   ├── Domain/
│   │   │   │   │   └── Entities/
│   │   │   │   │       └── PropertyTests.cs
│   │   │   │   └── Infrastructure/
│   │   │   │       └── Repositories/
│   │   │   │           └── PropertyRepositoryTests.cs
│   │   │   ├── IntegrationTests/
│   │   │   │   ├── Controllers/
│   │   │   │   │   └── PropertiesControllerTests.cs
│   │   │   │   └── Services/
│   │   │   │       └── EventPublisherTests.cs
│   │   │   ├── Helpers/
│   │   │   │   ├── TestDataFactory.cs
│   │   │   │   └── TestDatabaseFixture.cs
│   │   │   ├── PropertyService.Tests.csproj
│   │   │   ├── appsettings.Test.json
│   │   │   └── xunit.runner.json
│   │   ├── Dockerfile
│   │   ├── Dockerfile.dev
│   │   ├── .dockerignore
│   │   ├── .env.example
│   │   ├── PropertyService.sln
│   │   ├── Directory.Build.props
│   │   ├── stylecop.json
│   │   └── README.md
│   ├── InspectionService/
│   │   ├── InspectionService.API/
│   │   │   ├── Endpoints/
│   │   │   │   ├── InspectionEndpoints.cs
│   │   │   │   ├── HealthEndpoints.cs
│   │   │   │   └── MetricsEndpoints.cs
│   │   │   ├── Middleware/
│   │   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   │   ├── RequestLoggingMiddleware.cs
│   │   │   │   └── CorrelationIdMiddleware.cs
│   │   │   ├── Validators/
│   │   │   │   ├── CreateInspectionValidator.cs
│   │   │   │   └── UploadPhotoValidator.cs
│   │   │   ├── Program.cs
│   │   │   ├── Startup.cs
│   │   │   ├── appsettings.json
│   │   │   ├── appsettings.Development.json
│   │   │   ├── appsettings.Production.json
│   │   │   ├── appsettings.Docker.json
│   │   │   ├── InspectionService.API.csproj
│   │   │   └── Properties/
│   │   │       └── launchSettings.json
│   │   ├── InspectionService.Application/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateInspectionCommand.cs
│   │   │   │   ├── CompleteInspectionCommand.cs
│   │   │   │   ├── UploadInspectionPhotoCommand.cs
│   │   │   │   └── CancelInspectionCommand.cs
│   │   │   ├── Handlers/
│   │   │   │   ├── CreateInspectionHandler.cs
│   │   │   │   ├── CompleteInspectionHandler.cs
│   │   │   │   ├── UploadInspectionPhotoHandler.cs
│   │   │   │   └── CancelInspectionHandler.cs
│   │   │   ├── Queries/
│   │   │   │   ├── GetInspectionByIdQuery.cs
│   │   │   │   ├── GetAllInspectionsQuery.cs
│   │   │   │   ├── GetInspectionsByPropertyQuery.cs
│   │   │   │   └── GetInspectionPhotosQuery.cs
│   │   │   ├── DTOs/
│   │   │   │   ├── InspectionDto.cs
│   │   │   │   ├── CreateInspectionDto.cs
│   │   │   │   ├── CompleteInspectionDto.cs
│   │   │   │   └── InspectionPhotoDto.cs
│   │   │   ├── Mappings/
│   │   │   │   └── AutoMapperProfile.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── IInspectionService.cs
│   │   │   │   ├── IPhotoStorageService.cs
│   │   │   │   └── IEventPublisher.cs
│   │   │   ├── Services/
│   │   │   │   ├── InspectionService.cs
│   │   │   │   ├── PhotoStorageService.cs
│   │   │   │   └── EventPublisherService.cs
│   │   │   ├── Exceptions/
│   │   │   │   ├── InspectionNotFoundException.cs
│   │   │   │   ├── InspectionAlreadyCompletedException.cs
│   │   │   │   └── PhotoUploadException.cs
│   │   │   ├── Constants/
│   │   │   │   └── ErrorMessages.cs
│   │   │   ├── InspectionService.Application.csproj
│   │   │   └── DependencyInjection.cs
│   │   ├── InspectionService.Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── Inspection.cs
│   │   │   │   ├── InspectionPhoto.cs
│   │   │   │   ├── InspectionChecklist.cs
│   │   │   │   └── InspectionReport.cs
│   │   │   ├── Enums/
│   │   │   │   ├── InspectionStatus.cs
│   │   │   │   ├── InspectionType.cs
│   │   │   │   └── InspectionPriority.cs
│   │   │   ├── Events/
│   │   │   │   ├── InspectionCreatedEvent.cs
│   │   │   │   ├── InspectionCompletedEvent.cs
│   │   │   │   └── InspectionPhotoUploadedEvent.cs
│   │   │   ├── ValueObjects/
│   │   │   │   ├── InspectionAddress.cs
│   │   │   │   └── PhotoMetadata.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── IInspectionRepository.cs
│   │   │   │   ├── IPhotoRepository.cs
│   │   │   │   └── IUnitOfWork.cs
│   │   │   ├── Specifications/
│   │   │   │   ├── InspectionSpecification.cs
│   │   │   │   └── InspectionSearchSpecification.cs
│   │   │   ├── InspectionService.Domain.csproj
│   │   │   └── AssemblyInfo.cs
│   │   ├── InspectionService.Infrastructure/
│   │   │   ├── Data/
│   │   │   │   ├── InspectionDbContext.cs
│   │   │   │   ├── DesignTimeDbContextFactory.cs
│   │   │   │   ├── Configurations/
│   │   │   │   │   ├── InspectionConfiguration.cs
│   │   │   │   │   ├── InspectionPhotoConfiguration.cs
│   │   │   │   │   └── InspectionChecklistConfiguration.cs
│   │   │   │   ├── Migrations/
│   │   │   │   │   ├── 20240101000000_InitialCreate.cs
│   │   │   │   │   ├── 20240101000000_InitialCreate.Designer.cs
│   │   │   │   │   ├── 20240120000000_AddPhotoMetadata.cs
│   │   │   │   │   ├── 20240120000000_AddPhotoMetadata.Designer.cs
│   │   │   │   │   └── InspectionDbContextModelSnapshot.cs
│   │   │   │   └── SeedData/
│   │   │   │       └── InspectionSeedData.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── InspectionRepository.cs
│   │   │   │   ├── PhotoRepository.cs
│   │   │   │   └── UnitOfWork.cs
│   │   │   ├── Services/
│   │   │   │   ├── BlobStorageService.cs
│   │   │   │   ├── ServiceBusPublisher.cs
│   │   │   │   ├── ImageProcessorService.cs
│   │   │   │   └── ThumbnailGeneratorService.cs
│   │   │   ├── Extensions/
│   │   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   │   └── ConfigurationExtensions.cs
│   │   │   ├── Options/
│   │   │   │   ├── BlobStorageOptions.cs
│   │   │   │   ├── ServiceBusOptions.cs
│   │   │   │   └── ImageProcessingOptions.cs
│   │   │   ├── InspectionService.Infrastructure.csproj
│   │   │   └── AssemblyInfo.cs
│   │   ├── InspectionService.Tests/
│   │   │   ├── UnitTests/
│   │   │   │   ├── Application/
│   │   │   │   │   ├── Handlers/
│   │   │   │   │   │   ├── CreateInspectionHandlerTests.cs
│   │   │   │   │   │   └── CompleteInspectionHandlerTests.cs
│   │   │   │   │   └── Services/
│   │   │   │   │       └── InspectionServiceTests.cs
│   │   │   │   ├── Domain/
│   │   │   │   │   └── Entities/
│   │   │   │   │       └── InspectionTests.cs
│   │   │   │   └── Infrastructure/
│   │   │   │       └── Services/
│   │   │   │           └── BlobStorageServiceTests.cs
│   │   │   ├── IntegrationTests/
│   │   │   │   └── Endpoints/
│   │   │   │       └── InspectionEndpointsTests.cs
│   │   │   ├── Helpers/
│   │   │   │   ├── TestDataFactory.cs
│   │   │   │   └── TestDatabaseFixture.cs
│   │   │   ├── InspectionService.Tests.csproj
│   │   │   ├── appsettings.Test.json
│   │   │   └── xunit.runner.json
│   │   ├── Dockerfile
│   │   ├── Dockerfile.dev
│   │   ├── .dockerignore
│   │   ├── .env.example
│   │   ├── InspectionService.sln
│   │   ├── Directory.Build.props
│   │   ├── stylecop.json
│   │   └── README.md
│   ├── RiskScoringService/
│   │   ├── app/
│   │   │   ├── __init__.py
│   │   │   ├── main.py
│   │   │   ├── models.py
│   │   │   ├── schemas.py
│   │   │   ├── dependencies.py
│   │   │   ├── config.py
│   │   │   ├── api/
│   │   │   │   ├── __init__.py
│   │   │   │   ├── routes.py
│   │   │   │   ├── health.py
│   │   │   │   └── metrics.py
│   │   │   ├── core/
│   │   │   │   ├── __init__.py
│   │   │   │   ├── risk_engine.py
│   │   │   │   ├── scoring_algorithm.py
│   │   │   │   ├── factor_weights.py
│   │   │   │   ├── risk_calculator.py
│   │   │   │   └── recommendation_engine.py
│   │   │   ├── domain/
│   │   │   │   ├── __init__.py
│   │   │   │   ├── entities.py
│   │   │   │   ├── value_objects.py
│   │   │   │   └── enums.py
│   │   │   ├── infrastructure/
│   │   │   │   ├── __init__.py
│   │   │   │   ├── database.py
│   │   │   │   ├── service_bus.py
│   │   │   │   ├── cosmos_client.py
│   │   │   │   ├── http_client.py
│   │   │   │   ├── cache.py
│   │   │   │   └── repositories/
│   │   │   │       ├── __init__.py
│   │   │   │       ├── risk_score_repository.py
│   │   │   │       └── property_repository.py
│   │   │   ├── services/
│   │   │   │   ├── __init__.py
│   │   │   │   ├── risk_scoring_service.py
│   │   │   │   ├── inspection_service.py
│   │   │   │   ├── property_service.py
│   │   │   │   ├── notification_service.py
│   │   │   │   └── event_processor.py
│   │   │   ├── middleware/
│   │   │   │   ├── __init__.py
│   │   │   │   ├── correlation_id.py
│   │   │   │   ├── request_logging.py
│   │   │   │   ├── error_handler.py
│   │   │   │   └── authentication.py
│   │   │   ├── utils/
│   │   │   │   ├── __init__.py
│   │   │   │   ├── validators.py
│   │   │   │   ├── logger.py
│   │   │   │   ├── metrics.py
│   │   │   │   └── telemetry.py
│   │   │   └── constants/
│   │   │       ├── __init__.py
│   │   │       ├── error_messages.py
│   │   │       └── risk_thresholds.py
│   │   ├── tests/
│   │   │   ├── __init__.py
│   │   │   ├── test_main.py
│   │   │   ├── test_risk_engine.py
│   │   │   ├── test_scoring_algorithm.py
│   │   │   ├── test_api.py
│   │   │   ├── test_services.py
│   │   │   ├── test_repositories.py
│   │   │   ├── fixtures/
│   │   │   │   ├── __init__.py
│   │   │   │   ├── test_data.py
│   │   │   │   └── mock_clients.py
│   │   │   ├── integration/
│   │   │   │   ├── __init__.py
│   │   │   │   ├── test_service_bus.py
│   │   │   │   └── test_cosmos_db.py
│   │   │   └── conftest.py
│   │   ├── scripts/
│   │   │   ├── init_db.py
│   │   │   ├── seed_data.py
│   │   │   └── run_migrations.py
│   │   ├── Dockerfile
│   │   ├── Dockerfile.dev
│   │   ├── .dockerignore
│   │   ├── requirements.txt
│   │   ├── requirements-dev.txt
│   │   ├── pyproject.toml
│   │   ├── setup.py
│   │   ├── .env.example
│   │   ├── .flake8
│   │   ├── .pylintrc
│   │   ├── pytest.ini
│   │   ├── tox.ini
│   │   ├── Makefile
│   │   └── README.md
│   ├── DocumentService/
│   │   ├── DocumentService.API/
│   │   │   ├── Controllers/
│   │   │   │   ├── DocumentsController.cs
│   │   │   │   ├── ReportsController.cs
│   │   │   │   ├── HealthController.cs
│   │   │   │   └── MetricsController.cs
│   │   │   ├── Middleware/
│   │   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   │   ├── RequestLoggingMiddleware.cs
│   │   │   │   └── CorrelationIdMiddleware.cs
│   │   │   ├── Filters/
│   │   │   │   ├── ValidationFilter.cs
│   │   │   │   └── AuthorizeFilter.cs
│   │   │   ├── Validators/
│   │   │   │   ├── GenerateComplianceReportValidator.cs
│   │   │   │   └── GenerateInspectionReportValidator.cs
│   │   │   ├── Program.cs
│   │   │   ├── Startup.cs
│   │   │   ├── appsettings.json
│   │   │   ├── appsettings.Development.json
│   │   │   ├── appsettings.Production.json
│   │   │   ├── appsettings.Docker.json
│   │   │   ├── DocumentService.API.csproj
│   │   │   └── Properties/
│   │   │       └── launchSettings.json
│   │   ├── DocumentService.Application/
│   │   │   ├── Commands/
│   │   │   │   ├── GenerateComplianceReportCommand.cs
│   │   │   │   ├── GenerateInspectionReportCommand.cs
│   │   │   │   ├── DownloadDocumentCommand.cs
│   │   │   │   └── DeleteDocumentCommand.cs
│   │   │   ├── Handlers/
│   │   │   │   ├── GenerateComplianceReportHandler.cs
│   │   │   │   ├── GenerateInspectionReportHandler.cs
│   │   │   │   ├── DownloadDocumentHandler.cs
│   │   │   │   └── DeleteDocumentHandler.cs
│   │   │   ├── Queries/
│   │   │   │   ├── GetDocumentByIdQuery.cs
│   │   │   │   ├── GetDocumentsByPropertyQuery.cs
│   │   │   │   ├── GetDocumentsByTypeQuery.cs
│   │   │   │   └── SearchDocumentsQuery.cs
│   │   │   ├── DTOs/
│   │   │   │   ├── DocumentDto.cs
│   │   │   │   ├── GenerateReportDto.cs
│   │   │   │   └── DocumentSearchDto.cs
│   │   │   ├── Mappings/
│   │   │   │   └── AutoMapperProfile.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── IDocumentService.cs
│   │   │   │   ├── IReportGeneratorService.cs
│   │   │   │   └── IBlobStorageService.cs
│   │   │   ├── Services/
│   │   │   │   ├── DocumentService.cs
│   │   │   │   ├── ReportGeneratorService.cs
│   │   │   │   ├── ComplianceReportGenerator.cs
│   │   │   │   ├── InspectionReportGenerator.cs
│   │   │   │   └── BlobStorageService.cs
│   │   │   ├── Exceptions/
│   │   │   │   ├── DocumentNotFoundException.cs
│   │   │   │   ├── ReportGenerationException.cs
│   │   │   │   └── StorageException.cs
│   │   │   ├── Constants/
│   │   │   │   └── ErrorMessages.cs
│   │   │   ├── DocumentService.Application.csproj
│   │   │   └── DependencyInjection.cs
│   │   ├── DocumentService.Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── Document.cs
│   │   │   │   ├── ReportTemplate.cs
│   │   │   │   └── DocumentMetadata.cs
│   │   │   ├── Enums/
│   │   │   │   ├── DocumentType.cs
│   │   │   │   ├── ReportStatus.cs
│   │   │   │   └── DocumentFormat.cs
│   │   │   ├── Events/
│   │   │   │   ├── DocumentGeneratedEvent.cs
│   │   │   │   └── ReportCompletedEvent.cs
│   │   │   ├── ValueObjects/
│   │   │   │   ├── DocumentContent.cs
│   │   │   │   └── ReportParameters.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── IDocumentRepository.cs
│   │   │   │   ├── IReportTemplateRepository.cs
│   │   │   │   └── IUnitOfWork.cs
│   │   │   ├── Specifications/
│   │   │   │   ├── DocumentSpecification.cs
│   │   │   │   └── DocumentSearchSpecification.cs
│   │   │   ├── DocumentService.Domain.csproj
│   │   │   └── AssemblyInfo.cs
│   │   ├── DocumentService.Infrastructure/
│   │   │   ├── Data/
│   │   │   │   ├── DocumentDbContext.cs
│   │   │   │   ├── DesignTimeDbContextFactory.cs
│   │   │   │   ├── Configurations/
│   │   │   │   │   ├── DocumentConfiguration.cs
│   │   │   │   │   └── ReportTemplateConfiguration.cs
│   │   │   │   ├── Migrations/
│   │   │   │   │   ├── 20240101000000_InitialCreate.cs
│   │   │   │   │   ├── 20240101000000_InitialCreate.Designer.cs
│   │   │   │   │   ├── 20240130000000_AddReportTemplates.cs
│   │   │   │   │   ├── 20240130000000_AddReportTemplates.Designer.cs
│   │   │   │   │   └── DocumentDbContextModelSnapshot.cs
│   │   │   │   └── SeedData/
│   │   │   │       └── DocumentSeedData.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── DocumentRepository.cs
│   │   │   │   ├── ReportTemplateRepository.cs
│   │   │   │   └── UnitOfWork.cs
│   │   │   ├── Services/
│   │   │   │   ├── BlobStorageService.cs
│   │   │   │   ├── PdfGeneratorService.cs
│   │   │   │   ├── WordGeneratorService.cs
│   │   │   │   ├── ExcelGeneratorService.cs
│   │   │   │   └── ServiceBusPublisher.cs
│   │   │   ├── Extensions/
│   │   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   │   └── ConfigurationExtensions.cs
│   │   │   ├── Options/
│   │   │   │   ├── BlobStorageOptions.cs
│   │   │   │   ├── DocumentGenerationOptions.cs
│   │   │   │   └── ServiceBusOptions.cs
│   │   │   ├── DocumentService.Infrastructure.csproj
│   │   │   └── AssemblyInfo.cs
│   │   ├── DocumentService.Tests/
│   │   │   ├── UnitTests/
│   │   │   │   ├── Application/
│   │   │   │   │   ├── Handlers/
│   │   │   │   │   │   ├── GenerateComplianceReportHandlerTests.cs
│   │   │   │   │   │   └── GenerateInspectionReportHandlerTests.cs
│   │   │   │   │   └── Services/
│   │   │   │   │       └── DocumentServiceTests.cs
│   │   │   │   ├── Domain/
│   │   │   │   │   └── Entities/
│   │   │   │   │       └── DocumentTests.cs
│   │   │   │   └── Infrastructure/
│   │   │   │       └── Services/
│   │   │   │           ├── BlobStorageServiceTests.cs
│   │   │   │           └── PdfGeneratorServiceTests.cs
│   │   │   ├── IntegrationTests/
│   │   │   │   ├── Controllers/
│   │   │   │   │   └── DocumentsControllerTests.cs
│   │   │   │   └── Services/
│   │   │   │       └── ReportGeneratorTests.cs
│   │   │   ├── Helpers/
│   │   │   │   ├── TestDataFactory.cs
│   │   │   │   └── TestDatabaseFixture.cs
│   │   │   ├── DocumentService.Tests.csproj
│   │   │   ├── appsettings.Test.json
│   │   │   └── xunit.runner.json
│   │   ├── Dockerfile
│   │   ├── Dockerfile.dev
│   │   ├── .dockerignore
│   │   ├── .env.example
│   │   ├── DocumentService.sln
│   │   ├── Directory.Build.props
│   │   ├── stylecop.json
│   │   └── README.md
│   └── NotificationService/
│       ├── app/
│       │   ├── __init__.py
│       │   ├── main.py
│       │   ├── models.py
│       │   ├── schemas.py
│       │   ├── dependencies.py
│       │   ├── config.py
│       │   ├── api/
│       │   │   ├── __init__.py
│       │   │   ├── routes.py
│       │   │   ├── health.py
│       │   │   └── metrics.py
│       │   ├── core/
│       │   │   ├── __init__.py
│       │   │   ├── notification_engine.py
│       │   │   ├── template_engine.py
│       │   │   ├── channel_manager.py
│       │   │   └── rate_limiter.py
│       │   ├── domain/
│       │   │   ├── __init__.py
│       │   │   ├── entities.py
│       │   │   ├── value_objects.py
│       │   │   └── enums.py
│       │   ├── infrastructure/
│       │   │   ├── __init__.py
│       │   │   ├── service_bus.py
│       │   │   ├── email_client.py
│       │   │   ├── sms_client.py
│       │   │   ├── push_client.py
│       │   │   ├── database.py
│       │   │   └── cache.py
│       │   ├── services/
│       │   │   ├── __init__.py
│       │   │   ├── notification_service.py
│       │   │   ├── email_service.py
│       │   │   ├── sms_service.py
│       │   │   ├── push_service.py
│       │   │   └── event_processor.py
│       │   ├── middleware/
│       │   │   ├── __init__.py
│       │   │   ├── correlation_id.py
│       │   │   ├── request_logging.py
│       │   │   ├── error_handler.py
│       │   │   └── authentication.py
│       │   ├── templates/
│       │   │   ├── email/
│       │   │   │   ├── base.html
│       │   │   │   ├── property_created.html
│       │   │   │   ├── inspection_scheduled.html
│       │   │   │   ├── inspection_completed.html
│       │   │   │   ├── risk_score_updated.html
│       │   │   │   ├── report_generated.html
│       │   │   │   └── notification_preference.html
│       │   │   └── sms/
│       │   │       ├── property_created.txt
│       │   │       ├── inspection_scheduled.txt
│       │   │       ├── inspection_completed.txt
│       │   │       └── risk_score_updated.txt
│       │   ├── utils/
│       │   │   ├── __init__.py
│       │   │   ├── validators.py
│       │   │   ├── logger.py
│       │   │   ├── metrics.py
│       │   │   └── telemetry.py
│       │   └── constants/
│       │       ├── __init__.py
│       │       ├── error_messages.py
│       │       └── notification_types.py
│       ├── tests/
│       │   ├── __init__.py
│       │   ├── test_main.py
│       │   ├── test_notification_engine.py
│       │   ├── test_services.py
│       │   ├── test_api.py
│       │   ├── fixtures/
│       │   │   ├── __init__.py
│       │   │   ├── test_data.py
│       │   │   └── mock_clients.py
│       │   └── integration/
│       │       ├── __init__.py
│       │       └── test_service_bus.py
│       ├── scripts/
│       │   ├── init_db.py
│       │   └── send_test_notification.py
│       ├── Dockerfile
│       ├── Dockerfile.dev
│       ├── .dockerignore
│       ├── requirements.txt
│       ├── requirements-dev.txt
│       ├── pyproject.toml
│       ├── setup.py
│       ├── .env.example
│       ├── .flake8
│       ├── .pylintrc
│       ├── pytest.ini
│       ├── tox.ini
│       ├── Makefile
│       └── README.md
├── docker-compose.yml
├── docker-compose.local.yml
├── docker-compose.observability.yml
├── scripts/
│   ├── setup-local.sh
│   ├── deploy-aks.sh
│   ├── run-migrations.sh
│   ├── health-check.sh
│   ├── backup-databases.sh
│   ├── restore-databases.sh
│   └── generate-certificates.sh
├── docs/
│   ├── architecture/
│   │   ├── architecture-diagram.md
│   │   ├── data-flow.md
│   │   ├── event-sourcing.md
│   │   └── sequence-diagrams.md
│   ├── api/
│   │   ├── property-service-api.md
│   │   ├── inspection-service-api.md
│   │   ├── risk-scoring-service-api.md
│   │   ├── document-service-api.md
│   │   └── notification-service-api.md
│   ├── deployment/
│   │   ├── azure-deployment.md
│   │   ├── kubernetes-deployment.md
│   │   └── local-development.md
│   ├── security/
│   │   ├── authentication.md
│   │   ├── authorization.md
│   │   └── secrets-management.md
│   └── monitoring/
│       ├── observability.md
│       ├── alerting.md
│       └── dashboards.md
├── .env.example
├── .gitignore
├── .editorconfig
├── .pre-commit-config.yaml
├── LICENSE
└── README.md
```


