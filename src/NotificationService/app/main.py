import asyncio
import json
import logging
import os
from datetime import datetime
from typing import Optional
from enum import Enum

from fastapi import FastAPI, HTTPException, BackgroundTasks
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, EmailStr

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI(
    title="Notification Service",
    description="Sends notifications based on property inspection events",
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
        self.smtp_host = os.getenv("SMTP_HOST", "smtp.gmail.com")
        self.smtp_port = int(os.getenv("SMTP_PORT", "587"))
        self.smtp_username = os.getenv("SMTP_USERNAME", "")
        self.smtp_password = os.getenv("SMTP_PASSWORD", "")
        self.from_email = os.getenv("FROM_EMAIL", "noreply@property-platform.com")

settings = Settings()

# ── Models ─────────────────────────────────────────────────────────────────────

class NotificationChannel(str, Enum):
    EMAIL = "email"
    SMS = "sms"
    IN_APP = "in_app"

class NotificationStatus(str, Enum):
    PENDING = "pending"
    SENT = "sent"
    FAILED = "failed"

class NotificationRequest(BaseModel):
    recipient_id: str
    recipient_email: Optional[str] = None
    recipient_phone: Optional[str] = None
    subject: str
    message: str
    channel: NotificationChannel = NotificationChannel.EMAIL
    event_type: Optional[str] = None
    property_id: Optional[str] = None
    metadata: dict = {}

class NotificationRecord(BaseModel):
    id: str
    recipient_id: str
    subject: str
    message: str
    channel: str
    status: str
    event_type: Optional[str]
    property_id: Optional[str]
    created_at: str
    sent_at: Optional[str] = None
    error: Optional[str] = None

class NotificationEvent(BaseModel):
    event_type: str
    property_id: Optional[str] = None
    inspection_id: Optional[str] = None
    risk_score: Optional[float] = None
    risk_category: Optional[str] = None
    owner_id: Optional[str] = None
    inspector_id: Optional[str] = None
    timestamp: str = ""
    metadata: dict = {}

# ── In-memory store for demo ───────────────────────────────────────────────────

notifications_store: list[dict] = []

# ── Notification Templates ─────────────────────────────────────────────────────

TEMPLATES = {
    "PropertyCreated": {
        "subject": "New Property Registered",
        "message": "A new property has been registered in the system. Property ID: {property_id}"
    },
    "InspectionScheduled": {
        "subject": "Inspection Scheduled",
        "message": "An inspection has been scheduled for property {property_id}. Please check the portal for details."
    },
    "InspectionCompleted": {
        "subject": "Inspection Completed",
        "message": "The inspection for property {property_id} has been completed. View the full report in the portal."
    },
    "RiskScoreCalculated": {
        "subject": "Risk Score Updated",
        "message": "Risk assessment for property {property_id} is complete. Score: {risk_score}/100 ({risk_category})."
    },
    "ReportGenerated": {
        "subject": "Report Ready",
        "message": "Your compliance report for property {property_id} has been generated and is ready for download."
    }
}

# ── Services ───────────────────────────────────────────────────────────────────

async def send_email_notification(record: dict) -> bool:
    """Send email notification — logs to console in demo mode."""
    if not settings.smtp_username:
        logger.info(
            "[DEMO] Email notification sent\n"
            f"  To: {record.get('recipient_email', 'unknown')}\n"
            f"  Subject: {record['subject']}\n"
            f"  Message: {record['message']}"
        )
        return True

    try:
        import aiosmtplib
        from email.mime.text import MIMEText

        msg = MIMEText(record["message"])
        msg["Subject"] = record["subject"]
        msg["From"] = settings.from_email
        msg["To"] = record.get("recipient_email", "")

        await aiosmtplib.send(
            msg,
            hostname=settings.smtp_host,
            port=settings.smtp_port,
            username=settings.smtp_username,
            password=settings.smtp_password,
            start_tls=True,
        )
        return True
    except Exception as e:
        logger.error(f"Email send failed: {e}")
        return False


async def process_notification(record: dict) -> None:
    """Dispatch to the appropriate channel and update status."""
    try:
        success = False
        channel = record["channel"]

        if channel == NotificationChannel.EMAIL:
            success = await send_email_notification(record)
        elif channel == NotificationChannel.SMS:
            logger.info(f"[DEMO] SMS sent to {record.get('recipient_phone')}: {record['message']}")
            success = True
        else:
            logger.info(f"[DEMO] In-app notification: {record['message']}")
            success = True

        record["status"] = NotificationStatus.SENT if success else NotificationStatus.FAILED
        record["sent_at"] = datetime.utcnow().isoformat()
    except Exception as e:
        record["status"] = NotificationStatus.FAILED
        record["error"] = str(e)
        logger.error(f"Notification {record['id']} failed: {e}")

# ── Routes ─────────────────────────────────────────────────────────────────────

@app.post("/api/notifications", response_model=NotificationRecord, status_code=201)
async def send_notification(request: NotificationRequest, background_tasks: BackgroundTasks):
    """Send a notification via the specified channel."""
    record = {
        "id": f"notif-{datetime.utcnow().timestamp():.0f}",
        "recipient_id": request.recipient_id,
        "recipient_email": request.recipient_email,
        "recipient_phone": request.recipient_phone,
        "subject": request.subject,
        "message": request.message,
        "channel": request.channel,
        "status": NotificationStatus.PENDING,
        "event_type": request.event_type,
        "property_id": request.property_id,
        "metadata": request.metadata,
        "created_at": datetime.utcnow().isoformat(),
        "sent_at": None,
        "error": None,
    }

    notifications_store.append(record)
    background_tasks.add_task(process_notification, record)

    logger.info(f"Notification {record['id']} queued for {request.recipient_id}")
    return record


@app.post("/api/notifications/event")
async def handle_event(event: NotificationEvent, background_tasks: BackgroundTasks):
    """Process a platform event and send appropriate notifications."""
    template = TEMPLATES.get(event.event_type)
    if not template:
        raise HTTPException(status_code=400, detail=f"Unknown event type: {event.event_type}")

    context = {
        "property_id": event.property_id or "N/A",
        "inspection_id": event.inspection_id or "N/A",
        "risk_score": event.risk_score or 0,
        "risk_category": event.risk_category or "Unknown",
    }

    subject = template["subject"]
    message = template["message"].format(**context)

    record = {
        "id": f"evt-{datetime.utcnow().timestamp():.0f}",
        "recipient_id": event.owner_id or event.inspector_id or "system",
        "recipient_email": None,
        "subject": subject,
        "message": message,
        "channel": NotificationChannel.EMAIL,
        "status": NotificationStatus.PENDING,
        "event_type": event.event_type,
        "property_id": event.property_id,
        "metadata": event.metadata,
        "created_at": datetime.utcnow().isoformat(),
        "sent_at": None,
        "error": None,
    }

    notifications_store.append(record)
    background_tasks.add_task(process_notification, record)

    return {"status": "accepted", "notification_id": record["id"], "event_type": event.event_type}


@app.get("/api/notifications")
async def list_notifications(limit: int = 50, skip: int = 0):
    """List recent notifications."""
    return {
        "total": len(notifications_store),
        "items": notifications_store[skip: skip + limit]
    }


@app.get("/api/notifications/{notification_id}")
async def get_notification(notification_id: str):
    """Get a specific notification by ID."""
    for n in notifications_store:
        if n["id"] == notification_id:
            return n
    raise HTTPException(status_code=404, detail="Notification not found")


@app.get("/api/notifications/recipient/{recipient_id}")
async def get_notifications_by_recipient(recipient_id: str):
    """Get all notifications for a recipient."""
    return [n for n in notifications_store if n["recipient_id"] == recipient_id]


@app.get("/health")
async def health():
    return {
        "status": "healthy",
        "service": "NotificationService",
        "timestamp": datetime.utcnow().isoformat(),
        "notifications_sent": sum(1 for n in notifications_store if n["status"] == "sent")
    }


@app.get("/info")
async def info():
    return {
        "service": "Notification Service",
        "version": "1.0.0",
        "status": "Running",
        "smtp_configured": bool(settings.smtp_username),
        "service_bus_configured": bool(settings.service_bus_connection),
        "timestamp": datetime.utcnow().isoformat()
    }


@app.get("/api/notifications/templates")
async def get_templates():
    """List available notification templates."""
    return {"templates": list(TEMPLATES.keys())}


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
