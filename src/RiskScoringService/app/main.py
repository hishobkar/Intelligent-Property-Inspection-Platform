import asyncio
import json
import logging
import os
from typing import Optional
from datetime import datetime

from fastapi import FastAPI, HTTPException, BackgroundTasks
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from opentelemetry import trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.resources import SERVICE_NAME, Resource

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# OpenTelemetry — minimal setup that doesn't fail without an OTLP endpoint
resource = Resource(attributes={SERVICE_NAME: "RiskScoringService"})
provider = TracerProvider(resource=resource)
trace.set_tracer_provider(provider)
tracer = trace.get_tracer(__name__)

# FastAPI app
app = FastAPI(
    title="Risk Scoring Service",
    description="Intelligent property risk scoring service based on inspection data",
    version="1.0.0"
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ── Configuration ──────────────────────────────────────────────────────────────

class Settings:
    def __init__(self):
        self.service_bus_connection = os.getenv("SERVICE_BUS_CONNECTION", "")
        self.cosmos_db_connection = os.getenv("COSMOS_DB_CONNECTION", "")
        self.cosmos_db_name = os.getenv("COSMOS_DB_NAME", "RiskScoringDb")
        self.cosmos_container_name = os.getenv("COSMOS_CONTAINER_NAME", "RiskScores")
        self.inspection_service_url = os.getenv("INSPECTION_SERVICE_URL", "http://inspection-service:8080")
        self.property_service_url = os.getenv("PROPERTY_SERVICE_URL", "http://property-service:8080")

settings = Settings()

# ── In-memory store for demo ───────────────────────────────────────────────────

risk_scores_store: list[dict] = []

# ── Models ─────────────────────────────────────────────────────────────────────

class RiskScoreRequest(BaseModel):
    property_id: str
    inspection_id: str
    inspection_data: dict = {}
    property_details: dict = {}

class RiskFactor(BaseModel):
    factor: str
    score: float
    weight: float
    weighted_score: float

class RiskScoreResponse(BaseModel):
    property_id: str
    inspection_id: str
    risk_score: float
    risk_category: str
    factors: list[dict]
    recommendations: list[str]
    calculated_at: str

# ── Risk Scoring Engine ────────────────────────────────────────────────────────

class RiskScoringEngine:
    WEIGHTS = {
        "structural_condition": 0.30,
        "property_age": 0.15,
        "location_risk": 0.20,
        "property_type": 0.10,
        "previous_damage": 0.15,
        "maintenance_history": 0.10,
    }

    def calculate(self, inspection_data: dict, property_details: dict) -> dict:
        factors = []

        # Structural condition
        structural = inspection_data.get("structural_condition", {}).get("score", 70)
        factors.append(self._factor("Structural Condition", structural, "structural_condition"))

        # Property age (newer = better score)
        year_built = property_details.get("year_built", 1990)
        age = datetime.utcnow().year - year_built
        age_score = max(0, 100 - age * 1.5)
        factors.append(self._factor("Property Age", age_score, "property_age"))

        # Location risk
        location_score = self._location_score(inspection_data.get("location", {}))
        factors.append(self._factor("Location Risk", location_score, "location_risk"))

        # Property type
        type_score = self._type_score(property_details.get("type", "residential"))
        factors.append(self._factor("Property Type", type_score, "property_type"))

        # Previous damage
        damage = inspection_data.get("previous_damage", {}).get("score", 80)
        factors.append(self._factor("Previous Damage", damage, "previous_damage"))

        # Maintenance history
        maintenance = inspection_data.get("maintenance_history", {}).get("score", 75)
        factors.append(self._factor("Maintenance History", maintenance, "maintenance_history"))

        overall = sum(f["weighted_score"] for f in factors) / sum(self.WEIGHTS.values()) * 100
        overall = round(overall, 2)

        if overall >= 80:
            category = "Low Risk"
        elif overall >= 60:
            category = "Medium Risk"
        elif overall >= 40:
            category = "High Risk"
        else:
            category = "Critical Risk"

        return {
            "risk_score": overall,
            "risk_category": category,
            "factors": factors,
            "recommendations": self._recommendations(factors),
        }

    def _factor(self, name: str, score: float, key: str) -> dict:
        score = max(0.0, min(100.0, float(score)))
        weight = self.WEIGHTS[key]
        return {
            "factor": name,
            "score": round(score, 2),
            "weight": weight,
            "weighted_score": round((score / 100) * weight, 4),
        }

    def _location_score(self, location: dict) -> float:
        score = 75.0
        if location.get("flood_zone") == "high":
            score -= 20
        elif location.get("flood_zone") == "moderate":
            score -= 10
        crime_rate = location.get("crime_rate", 0)
        if crime_rate > 50:
            score -= 15
        elif crime_rate > 30:
            score -= 10
        if location.get("near_hazardous_site", False):
            score -= 15
        return max(0.0, min(100.0, score))

    def _type_score(self, property_type: str) -> float:
        return {"residential": 85, "commercial": 75, "industrial": 65, "mixed_use": 70, "land": 80}.get(
            property_type.lower(), 70
        )

    def _recommendations(self, factors: list[dict]) -> list[str]:
        recs = []
        for f in factors:
            if f["score"] < 60:
                if "Structural" in f["factor"]:
                    recs.append("Conduct detailed structural engineering assessment")
                elif "Age" in f["factor"]:
                    recs.append("Consider property renovation or modernisation plan")
                elif "Location" in f["factor"]:
                    recs.append("Review location risk mitigation strategies")
                elif "Damage" in f["factor"]:
                    recs.append("Develop comprehensive repair and maintenance plan")
                elif "Maintenance" in f["factor"]:
                    recs.append("Implement proactive maintenance programme")
        if not recs:
            recs.append("Property appears in good condition — maintain regular inspection schedule")
        return recs[:5]


engine = RiskScoringEngine()

# ── Routes ─────────────────────────────────────────────────────────────────────

@app.post("/api/risk/score", response_model=RiskScoreResponse)
async def calculate_risk_score(request: RiskScoreRequest):
    """Calculate risk score for a property based on inspection data."""
    with tracer.start_as_current_span("calculate_risk_score") as span:
        span.set_attribute("property_id", request.property_id)
        span.set_attribute("inspection_id", request.inspection_id)

        result = engine.calculate(request.inspection_data, request.property_details)
        calculated_at = datetime.utcnow().isoformat()

        record = {
            "id": f"risk-{datetime.utcnow().timestamp():.0f}",
            "property_id": request.property_id,
            "inspection_id": request.inspection_id,
            "risk_score": result["risk_score"],
            "risk_category": result["risk_category"],
            "factors": result["factors"],
            "recommendations": result["recommendations"],
            "inspection_data": request.inspection_data,
            "calculated_at": calculated_at,
        }

        # Persist to Cosmos DB if configured, otherwise use in-memory store
        if settings.cosmos_db_connection:
            try:
                from azure.cosmos.aio import CosmosClient
                from azure.cosmos import PartitionKey

                async with CosmosClient.from_connection_string(settings.cosmos_db_connection) as client:
                    database = client.get_database_client(settings.cosmos_db_name)
                    container = database.get_container_client(settings.cosmos_container_name)
                    await container.create_item(record)
                    logger.info("Risk score stored in Cosmos DB for property %s", request.property_id)
            except Exception as e:
                logger.warning("Cosmos DB unavailable, using in-memory store: %s", e)
                risk_scores_store.append(record)
        else:
            risk_scores_store.append(record)
            logger.info("Risk score stored in-memory for property %s", request.property_id)

        return {**result, "property_id": request.property_id, "inspection_id": request.inspection_id, "calculated_at": calculated_at}


@app.get("/api/risk/scores")
async def list_risk_scores(limit: int = 50, skip: int = 0):
    """List recent risk scores (in-memory demo store)."""
    return {
        "total": len(risk_scores_store),
        "items": risk_scores_store[skip: skip + limit]
    }


@app.get("/api/risk/scores/{property_id}")
async def get_risk_scores_by_property(property_id: str):
    """Get risk scores for a specific property."""
    scores = [s for s in risk_scores_store if s["property_id"] == property_id]
    return {"property_id": property_id, "scores": scores}


@app.post("/api/risk/process-inspection")
async def process_inspection_event(payload: dict, background_tasks: BackgroundTasks):
    """Handle InspectionCompleted event — triggers risk calculation."""
    background_tasks.add_task(_handle_inspection_completed, payload)
    return {"status": "accepted", "event_type": payload.get("event_type")}


async def _handle_inspection_completed(payload: dict):
    """Fetch inspection+property data and calculate risk score."""
    inspection_id = payload.get("inspection_id")
    property_id = payload.get("property_id")

    try:
        import httpx
        async with httpx.AsyncClient(timeout=10.0) as client:
            inspection_resp = await client.get(f"{settings.inspection_service_url}/api/inspections/{inspection_id}")
            inspection_data = inspection_resp.json() if inspection_resp.status_code == 200 else {}

            property_resp = await client.get(f"{settings.property_service_url}/api/properties/{property_id}")
            property_data = property_resp.json() if property_resp.status_code == 200 else {}
    except Exception as e:
        logger.warning("Could not fetch service data: %s. Using empty context.", e)
        inspection_data = {}
        property_data = {}

    req = RiskScoreRequest(
        property_id=str(property_id),
        inspection_id=str(inspection_id),
        inspection_data=inspection_data,
        property_details=property_data,
    )
    result = await calculate_risk_score(req)
    logger.info("Auto-calculated risk score %.1f for inspection %s", result["risk_score"], inspection_id)


@app.get("/health")
async def health():
    return {
        "status": "healthy",
        "service": "RiskScoringService",
        "cosmos_configured": bool(settings.cosmos_db_connection),
        "scores_calculated": len(risk_scores_store),
        "timestamp": datetime.utcnow().isoformat(),
    }


@app.get("/info")
async def info():
    return {
        "service": "Risk Scoring Service",
        "version": "1.0.0",
        "status": "Running",
        "algorithm": "Weighted multi-factor scoring",
        "factors": list(RiskScoringEngine.WEIGHTS.keys()),
        "timestamp": datetime.utcnow().isoformat(),
    }


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
