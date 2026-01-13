from fastapi import APIRouter, HTTPException

from app.services.patient_service import PatientService
from app.services.encounter_service import EncounterService
from app.services.practitioner_service import PractitionerService
from app.adapter.fhir_event_builder import build_external_event
from app.adapter.fhir_client import send_to_fhir_platform

router = APIRouter(prefix="/integration", tags=["Integration"])


@router.post("/send/{patient_uuid}")
def send_patient_encounter(patient_uuid: str):
    PatientService.seed_demo_data()
    PractitionerService.seed_demo_data()
    EncounterService.seed_demo_data()

    patient = PatientService.get_by_uuid(patient_uuid)
    if not patient:
        raise HTTPException(status_code=404, detail="Patient not found")

    encounters = EncounterService.get_by_patient_uuid(patient_uuid)
    if not encounters:
        raise HTTPException(status_code=404, detail="No encounters found")

    practitioner = PractitionerService.get_primary_provider()
    if not practitioner:
        raise HTTPException(status_code=500, detail="Practitioner not available")

    event = build_external_event(
        patient=patient,
        encounter=encounters[0],
        practitioner=practitioner
    )

    response = send_to_fhir_platform(event.model_dump(mode="json"))

    return {
        "status": "sent",
        "fhir_response": response
    }
