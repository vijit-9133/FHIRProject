from fastapi import FastAPI

from app.api.ws.rest.v1.patient import router as patient_router
from app.api.ws.rest.v1.encounter import router as encounter_router
from app.api.ws.rest.v1.integration import router as integration_router
from app.api.ws.rest.v1.provider import router as provider_router



app = FastAPI(
    title="OpenMRS-like EHR Simulator",
    version="1.0.0",
    description="Python-based EHR that mirrors OpenMRS REST behavior"
)

app.include_router(patient_router)
app.include_router(encounter_router)
app.include_router(integration_router)
app.include_router(provider_router)





@app.get("/")
def root():
    return {
        "service": "ehr-simulator",
        "status": "running"
    }
