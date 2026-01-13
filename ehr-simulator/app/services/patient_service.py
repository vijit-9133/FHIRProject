from datetime import date
from typing import List, Optional

from app.models.patient import PatientModel


class PatientService:
    _patients: List[PatientModel] = []

    @classmethod
    def seed_demo_data(cls):
        if cls._patients:
            return  

        cls._patients = [
            PatientModel(
                uuid="14b0c1fb-1884-4f31-a4b8-a94eea477076",
                mrn="MRN-1001",
                given_name="John",
                family_name="Doe",
                gender="M",
                birthdate=date(1985, 3, 15),
                phone="+1-555-123-4567",
                email="john.doe@example.com"
            ),
            PatientModel(
                uuid="094f5178-d5bd-42d7-85af-9dcce2798b8b",
                mrn="MRN-1002",
                given_name="Jane",
                family_name="Smith",
                gender="F",
                birthdate=date(1990, 7, 22),
                phone="+1-555-987-6543",
                email="jane.smith@example.com"
            )
        ]

    @classmethod
    def get_all(cls):
        return cls._patients

    @classmethod
    def get_by_uuid(cls, uuid: str):
        return next((p for p in cls._patients if p.uuid == uuid), None)

    @classmethod
    def get_by_mrn(cls, mrn: str):
        return next((p for p in cls._patients if p.mrn == mrn), None)
