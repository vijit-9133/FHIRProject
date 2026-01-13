import uuid
from datetime import date
from typing import Optional

from pydantic import BaseModel


class PatientModel(BaseModel):
    """
    Internal representation of an OpenMRS Patient.
    This is NOT FHIR.
    """
    uuid: str
    mrn: str

    given_name: str
    family_name: str

    gender: str          # M / F / O
    birthdate: date

    phone: Optional[str] = None
    email: Optional[str] = None

    @staticmethod
    def create(
        mrn: str,
        given_name: str,
        family_name: str,
        gender: str,
        birthdate: date,
        phone: Optional[str] = None,
        email: Optional[str] = None
    ) -> "PatientModel":
        return PatientModel(
            uuid=str(uuid.uuid4()),
            mrn=mrn,
            given_name=given_name,
            family_name=family_name,
            gender=gender,
            birthdate=birthdate,
            phone=phone,
            email=email
        )
