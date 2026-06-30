# Azure Deployment Guide — Terraform + AKS (Production)

## What the Terraform Provisions

| Resource | Purpose |
|---|---|
| AKS cluster | Runs all 5 microservices |
| Azure Container Registry | Stores Docker images |
| Azure SQL | PropertyDb + InspectionDb (replaces SQLite) |
| Cosmos DB | RiskScoringDb |
| Service Bus | Event queues between services |
| Application Gateway (WAF v2) | Public entry point |
| API Management | API gateway layer |
| Key Vault | Secrets management |
| Application Insights + Log Analytics | Monitoring |

> **Cost warning:** This stack (WAF_v2 + APIM Developer + AKS 2-node minimum) runs **~$400–600/month**.
> If this is a portfolio/demo project, see [azure-deployment-guide-container-apps.md](azure-deployment-guide-container-apps.md) for the cheaper path (~$5–20/month).

---

## Prerequisites

```powershell
winget install Microsoft.AzureCLI
winget install Hashicorp.Terraform
winget install Docker.DockerDesktop
```

---

## Step 1 — Log in to Azure

```powershell
az login
az account show   # confirm the right subscription is active
az account set --subscription "YOUR_SUBSCRIPTION_ID"
```

---

## Step 2 — Create the Terraform State Backend (one-time)

The `main.tf` backend block requires a pre-existing storage account:

```powershell
az group create --name terraform-state-rg --location eastus

az storage account create `
  --name terraformstatestorage `
  --resource-group terraform-state-rg `
  --location eastus `
  --sku Standard_LRS

az storage container create `
  --name terraform-state `
  --account-name terraformstatestorage
```

---

## Step 3 — Create Missing Terraform Files

The `main.tf` references `var.*` values but no `variables.tf` exists in the repo. Create both:

### `infrastructure/terraform/variables.tf`

```hcl
variable "resource_group_name"      { type = string }
variable "location"                 { type = string  default = "eastus" }
variable "tags"                     { type = map(string) default = {} }
variable "acr_name"                 { type = string }
variable "aks_name"                 { type = string }
variable "node_count"               { type = number  default = 2 }
variable "node_vm_size"             { type = string  default = "Standard_D2s_v3" }
variable "apim_name"                { type = string }
variable "sql_server_name"          { type = string }
variable "sql_admin_username"       { type = string }
variable "sql_admin_password"       { type = string  sensitive = true }
variable "cosmos_account_name"      { type = string }
variable "storage_account_name"     { type = string }
variable "servicebus_namespace"     { type = string }
variable "event_grid_topic"         { type = string }
variable "keyvault_name"            { type = string }
variable "app_insights_name"        { type = string }
variable "log_analytics_workspace"  { type = string }
```

### `infrastructure/terraform/terraform.tfvars`

```hcl
resource_group_name      = "pip-rg"
location                 = "eastus"
acr_name                 = "pipcontainerregistry"   # must be globally unique, letters/numbers only
aks_name                 = "pip-aks"
apim_name                = "pip-apim"
sql_server_name          = "pip-sql-server"         # must be globally unique
sql_admin_username       = "pipadmin"
sql_admin_password       = "P@ssw0rd!Secure123"     # change this before applying
cosmos_account_name      = "pip-cosmos"             # must be globally unique
storage_account_name     = "pipblobstorage"         # must be globally unique
servicebus_namespace     = "pip-servicebus"         # must be globally unique
event_grid_topic         = "pip-events"
keyvault_name            = "pip-keyvault"           # must be globally unique
app_insights_name        = "pip-appinsights"
log_analytics_workspace  = "pip-logs"

tags = {
  project     = "intelligent-property-inspection"
  environment = "production"
}
```

---

## Step 4 — Run Terraform

```powershell
cd infrastructure/terraform

terraform init
terraform plan -out=tfplan
terraform apply tfplan
```

> This takes **15–25 minutes** (APIM and AKS are the slow resources).

---

## Step 5 — Build and Push Docker Images to ACR

```powershell
# Get ACR login server from Terraform output
$ACR = terraform output -raw acr_login_server   # e.g. pipcontainerregistry.azurecr.io

az acr login --name pipcontainerregistry

# Build and push each service from repo root
cd ../..

docker build -t $ACR/property-service:latest    ./src/PropertyService
docker build -t $ACR/inspection-service:latest  ./src/InspectionService
docker build -t $ACR/document-service:latest    ./src/DocumentService
docker build -t $ACR/risk-scoring-service:latest ./src/RiskScoringService
docker build -t $ACR/notification-service:latest ./src/NotificationService

docker push $ACR/property-service:latest
docker push $ACR/inspection-service:latest
docker push $ACR/document-service:latest
docker push $ACR/risk-scoring-service:latest
docker push $ACR/notification-service:latest
```

---

## Step 6 — Connect kubectl to AKS

```powershell
az aks get-credentials --resource-group pip-rg --name pip-aks
kubectl get nodes   # should show 2 nodes ready
```

---

## Step 7 — Grant AKS Permission to Pull from ACR

```powershell
az aks update --name pip-aks --resource-group pip-rg --attach-acr pipcontainerregistry
```

---

## Step 8 — Deploy Services to AKS

Create Kubernetes manifests under `infrastructure/k8s/`. Example for one service:

```yaml
# infrastructure/k8s/property-service.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: property-service
spec:
  replicas: 1
  selector:
    matchLabels:
      app: property-service
  template:
    metadata:
      labels:
        app: property-service
    spec:
      containers:
      - name: property-service
        image: pipcontainerregistry.azurecr.io/property-service:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
---
apiVersion: v1
kind: Service
metadata:
  name: property-service
spec:
  selector:
    app: property-service
  ports:
  - port: 80
    targetPort: 8080
```

Repeat the same pattern for `inspection-service`, `document-service`, `risk-scoring-service`, and `notification-service`.

```powershell
kubectl apply -f infrastructure/k8s/
kubectl get pods   # verify all pods are Running
```

---

## Step 9 — Verify Deployment

```powershell
# Check all pods are running
kubectl get pods --all-namespaces

# Check services have cluster IPs
kubectl get services

# View logs for a specific service
kubectl logs -l app=property-service --tail=50
```

---

## Tear Down (to avoid costs)

```powershell
cd infrastructure/terraform
terraform destroy
```

---

## Architecture Diagram

```
Internet
   │
   ▼
Application Gateway (WAF v2)
   │
   ▼
API Management
   │
   ├──► AKS: property-service    ──► Azure SQL (PropertyDb)
   ├──► AKS: inspection-service  ──► Azure SQL (InspectionDb)
   ├──► AKS: document-service    ──► Blob Storage
   ├──► AKS: risk-scoring-service ──► Cosmos DB (RiskScoringDb)
   └──► AKS: notification-service
              │
              └──► Service Bus (event queues)
                   Key Vault (secrets)
                   Application Insights (monitoring)
```
