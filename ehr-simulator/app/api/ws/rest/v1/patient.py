from typing import List, Optional

from fastapi import APIRouter, HTTPException, Query

from app.schemas.patient import (
    OpenMRSPatient,
    OpenMRSIdentifier,
    OpenMRSPerson,
    OpenMRSPersonName,
)
from app.services.patient_service import PatientService

router = APIRouter(prefix="/ws/rest/v1/patient", tags=["OpenMRS Patient"])


def to_openmrs_patient(patient) -> OpenMRSPatient:
    """
    Converts internal PatientModel to OpenMRS-style response.
    """
    return OpenMRSPatient(
        uuid=patient.uuid,
        identifiers=[
            OpenMRSIdentifier(identifier=patient.mrn)
        ],
        person=OpenMRSPerson(
            preferredName=OpenMRSPersonName(
                givenName=patient.given_name,
                familyName=patient.family_name
            ),
            gender=patient.gender,
            birthdate=patient.birthdate
        )
    )


@router.get("", response_model=List[OpenMRSPatient])
def get_patients(identifier: Optional[str] = Query(default=None)):
    """
    OpenMRS-style patient search.
    Supports:
      - GET /patient
      - GET /patient?identifier=MRN-1001
    """
    PatientService.seed_demo_data()

    if identifier:
        patient = PatientService.get_by_mrn(identifier)
        if not patient:
            return []
        return [to_openmrs_patient(patient)]

    return [to_openmrs_patient(p) for p in PatientService.get_all()]


@router.get("/{uuid}", response_model=OpenMRSPatient)
def get_patient_by_uuid(uuid: str):
    """
    OpenMRS-style patient fetch by UUID.
    """
    PatientService.seed_demo_data()

    patient = PatientService.get_by_uuid(uuid)
    if not patient:
        raise HTTPException(status_code=404, detail="Patient not found")

    return to_openmrs_patient(patient)
