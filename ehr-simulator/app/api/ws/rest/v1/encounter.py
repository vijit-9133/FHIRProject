from typing import List

from fastapi import APIRouter, Query

from app.schemas.encounter import OpenMRSEncounter
from app.services.encounter_service import EncounterService

router = APIRouter(prefix="/ws/rest/v1/encounter", tags=["OpenMRS Encounter"])


@router.get("", response_model=List[OpenMRSEncounter])
def get_encounters(patient: str = Query(...)):
    """
    OpenMRS-style encounter search:
    GET /ws/rest/v1/encounter?patient={patientUuid}
    """
    EncounterService.seed_demo_data()

    encounters = EncounterService.get_by_patient_uuid(patient)

    return [
        OpenMRSEncounter(
            uuid=e.uuid,
            encounterType=e.encounter_type,
            patient={"uuid": e.patient_uuid},
            encounterDatetime=e.start_datetime,
            reason=e.reason,
            location=e.location
        )
        for e in encounters
    ]
