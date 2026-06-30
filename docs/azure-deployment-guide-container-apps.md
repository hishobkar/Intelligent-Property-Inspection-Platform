# Azure Deployment Guide — Azure Container Apps (Portfolio / Demo)

## Why Container Apps Instead of AKS?

Azure Container Apps is the recommended path for portfolio demos and cost-conscious deployments.

| | AKS + Full Terraform | Azure Container Apps |
|---|---|---|
| Estimated monthly cost | $400–600 | $5–20 |
| Setup time | 25–40 min | 10–15 min |
| Kubernetes knowledge needed | Yes | No |
| Public HTTPS URL | Yes (via App Gateway) | Yes (built-in) |
| Scales to zero | No | Yes |
| Good for CV/portfolio | Yes (shows architecture) | Yes (shows working app) |

> Use the [AKS guide](azure-deployment-guide-terraform-aks.md) when you want to demonstrate the full production architecture. Use this guide when you want a live running demo URL to share.

---

## Prerequisites

```powershell
winget install Microsoft.AzureCLI
winget install Docker.DockerDesktop

# Install the Container Apps CLI extension
az extension add --name containerapp --upgrade
```

---

## Step 1 — Log in to Azure

```powershell
az login
az account show   # confirm the right subscription
az account set --subscription "YOUR_SUBSCRIPTION_ID"
```

---

## Step 2 — Create Resource Group and Container Apps Environment

```powershell
$RESOURCE_GROUP = "pip-demo-rg"
$LOCATION       = "eastus"
$ENV_NAME       = "pip-env"

az group create --name $RESOURCE_GROUP --location $LOCATION

az containerapp env create `
  --name $ENV_NAME `
  --resource-group $RESOURCE_GROUP `
  --location $LOCATION
```

---

## Step 3 — Create Azure Container Registry

```powershell
$ACR_NAME = "pipdemoacr"   # must be globally unique, letters/numbers only

az acr create `
  --name $ACR_NAME `
  --resource-group $RESOURCE_GROUP `
  --sku Basic `
  --admin-enabled true

$ACR_SERVER = az acr show --name $ACR_NAME --query loginServer -o tsv
$ACR_USER   = az acr credential show --name $ACR_NAME --query username -o tsv
$ACR_PASS   = az acr credential show --name $ACR_NAME --query "passwords[0].value" -o tsv
```

---

## Step 4 — Build and Push Docker Images

Run from the repository root:

```powershell
az acr login --name $ACR_NAME

docker build -t $ACR_SERVER/property-service:latest    ./src/PropertyService
docker build -t $ACR_SERVER/inspection-service:latest  ./src/InspectionService
docker build -t $ACR_SERVER/document-service:latest    ./src/DocumentService
docker build -t $ACR_SERVER/risk-scoring-service:latest ./src/RiskScoringService
docker build -t $ACR_SERVER/notification-service:latest ./src/NotificationService

docker push $ACR_SERVER/property-service:latest
docker push $ACR_SERVER/inspection-service:latest
docker push $ACR_SERVER/document-service:latest
docker push $ACR_SERVER/risk-scoring-service:latest
docker push $ACR_SERVER/notification-service:latest
```

---

## Step 5 — Deploy Each Service as a Container App

### Property Service (ASP.NET Core)

```powershell
az containerapp create `
  --name property-service `
  --resource-group $RESOURCE_GROUP `
  --environment $ENV_NAME `
  --image $ACR_SERVER/property-service:latest `
  --registry-server $ACR_SERVER `
  --registry-username $ACR_USER `
  --registry-password $ACR_PASS `
  --target-port 8080 `
  --ingress external `
  --min-replicas 0 `
  --max-replicas 3 `
  --env-vars "ASPNETCORE_ENVIRONMENT=Production" "ASPNETCORE_URLS=http://+:8080"
```

### Inspection Service (ASP.NET Core)

```powershell
az containerapp create `
  --name inspection-service `
  --resource-group $RESOURCE_GROUP `
  --environment $ENV_NAME `
  --image $ACR_SERVER/inspection-service:latest `
  --registry-server $ACR_SERVER `
  --registry-username $ACR_USER `
  --registry-password $ACR_PASS `
  --target-port 8080 `
  --ingress internal `
  --min-replicas 0 `
  --max-replicas 3 `
  --env-vars "ASPNETCORE_ENVIRONMENT=Production" "ASPNETCORE_URLS=http://+:8080"
```

### Document Service (ASP.NET Core)

```powershell
az containerapp create `
  --name document-service `
  --resource-group $RESOURCE_GROUP `
  --environment $ENV_NAME `
  --image $ACR_SERVER/document-service:latest `
  --registry-server $ACR_SERVER `
  --registry-username $ACR_USER `
  --registry-password $ACR_PASS `
  --target-port 8080 `
  --ingress internal `
  --min-replicas 0 `
  --max-replicas 3 `
  --env-vars "ASPNETCORE_ENVIRONMENT=Production" "ASPNETCORE_URLS=http://+:8080"
```

### Risk Scoring Service (Python FastAPI)

```powershell
# Get internal URLs for service-to-service calls
$INSPECTION_URL = az containerapp show `
  --name inspection-service `
  --resource-group $RESOURCE_GROUP `
  --query "properties.configuration.ingress.fqdn" -o tsv

$PROPERTY_URL = az containerapp show `
  --name property-service `
  --resource-group $RESOURCE_GROUP `
  --query "properties.configuration.ingress.fqdn" -o tsv

az containerapp create `
  --name risk-scoring-service `
  --resource-group $RESOURCE_GROUP `
  --environment $ENV_NAME `
  --image $ACR_SERVER/risk-scoring-service:latest `
  --registry-server $ACR_SERVER `
  --registry-username $ACR_USER `
  --registry-password $ACR_PASS `
  --target-port 8000 `
  --ingress internal `
  --min-replicas 0 `
  --max-replicas 3 `
  --env-vars `
    "PYTHONUNBUFFERED=1" `
    "INSPECTION_SERVICE_URL=https://$INSPECTION_URL" `
    "PROPERTY_SERVICE_URL=https://$PROPERTY_URL"
```

### Notification Service (Python FastAPI)

```powershell
az containerapp create `
  --name notification-service `
  --resource-group $RESOURCE_GROUP `
  --environment $ENV_NAME `
  --image $ACR_SERVER/notification-service:latest `
  --registry-server $ACR_SERVER `
  --registry-username $ACR_USER `
  --registry-password $ACR_PASS `
  --target-port 8000 `
  --ingress internal `
  --min-replicas 0 `
  --max-replicas 3 `
  --env-vars "PYTHONUNBUFFERED=1"
```

---

## Step 6 — Get Public URLs

```powershell
# Property service is the only externally exposed service
$PROPERTY_PUBLIC_URL = az containerapp show `
  --name property-service `
  --resource-group $RESOURCE_GROUP `
  --query "properties.configuration.ingress.fqdn" -o tsv

Write-Host "Property Service: https://$PROPERTY_PUBLIC_URL"
Write-Host "Swagger UI:       https://$PROPERTY_PUBLIC_URL/swagger"
```

---

## Step 7 — Add Optional Persistent Storage (SQLite → Azure SQL)

The local Docker Compose uses SQLite file volumes. For a persistent demo on Container Apps, add Azure SQL:

```powershell
$SQL_SERVER = "pip-demo-sql"
$SQL_DB     = "PropertyDb"
$SQL_USER   = "pipadmin"
$SQL_PASS   = "P@ssw0rd!Demo123"   # change this

az sql server create `
  --name $SQL_SERVER `
  --resource-group $RESOURCE_GROUP `
  --location $LOCATION `
  --admin-user $SQL_USER `
  --admin-password $SQL_PASS

az sql db create `
  --server $SQL_SERVER `
  --resource-group $RESOURCE_GROUP `
  --name $SQL_DB `
  --service-objective Basic

# Allow Azure services to connect
az sql server firewall-rule create `
  --server $SQL_SERVER `
  --resource-group $RESOURCE_GROUP `
  --name AllowAzureServices `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 0.0.0.0

$SQL_CONN = "Server=tcp:$SQL_SERVER.database.windows.net,1433;Database=$SQL_DB;User ID=$SQL_USER;Password=$SQL_PASS;Encrypt=True;"

# Update the property-service with the real connection string
az containerapp update `
  --name property-service `
  --resource-group $RESOURCE_GROUP `
  --set-env-vars "ConnectionStrings__DefaultConnection=$SQL_CONN"
```

---

## Step 8 — View Logs

```powershell
# Stream live logs for any service
az containerapp logs show `
  --name property-service `
  --resource-group $RESOURCE_GROUP `
  --follow

# View logs for risk-scoring-service
az containerapp logs show `
  --name risk-scoring-service `
  --resource-group $RESOURCE_GROUP `
  --tail 50
```

---

## Step 9 — Update a Service (Redeploy)

```powershell
# Rebuild and push a new image, then update the container app
docker build -t $ACR_SERVER/property-service:latest ./src/PropertyService
docker push $ACR_SERVER/property-service:latest

az containerapp update `
  --name property-service `
  --resource-group $RESOURCE_GROUP `
  --image $ACR_SERVER/property-service:latest
```

---

## Tear Down (to stop all charges)

```powershell
az group delete --name $RESOURCE_GROUP --yes --no-wait
```

This deletes everything in the resource group — all container apps, ACR, and optionally SQL.

---

## Architecture on Container Apps

```
Internet
   │
   ▼  (HTTPS, auto-cert)
property-service (external ingress)
   │
   ├──► inspection-service  (internal)  ──► SQLite / Azure SQL
   ├──► document-service    (internal)  ──► Blob Storage (optional)
   ├──► risk-scoring-service (internal)
   └──► notification-service (internal)

All services run in: pip-env (Container Apps Environment)
Images stored in:    pipdemoacr (Azure Container Registry)
```

---

## Quick Reference — Useful Commands

```powershell
# List all container apps
az containerapp list --resource-group $RESOURCE_GROUP -o table

# Check replica/scale status
az containerapp show --name property-service --resource-group $RESOURCE_GROUP --query "properties.runningStatus"

# Open Swagger for property service in browser
Start-Process "https://$(az containerapp show --name property-service --resource-group $RESOURCE_GROUP --query 'properties.configuration.ingress.fqdn' -o tsv)/swagger"
```
