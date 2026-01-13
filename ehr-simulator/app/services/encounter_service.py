from typing import List

from app.models.encounter import EncounterModel
from app.services.patient_service import PatientService


class EncounterService:
    """
    Simulates OpenMRS encounter storage.
    """
    _encounters: List[EncounterModel] = []

    @classmethod
    def seed_demo_data(cls):
        if cls._encounters:
            return

        PatientService.seed_demo_data()
        patients = PatientService.get_all()

        for patient in patients:
            cls._encounters.append(
                EncounterModel.create(
                    patient_uuid=patient.uuid,
                    encounter_type="Outpatient Visit",
                    reason="General consultation",
                    location="Main Hospital"
                )
            )

    @classmethod
    def get_by_patient_uuid(cls, patient_uuid: str) -> List[EncounterModel]:
        return [e for e in cls._encounters if e.patient_uuid == patient_uuid]
