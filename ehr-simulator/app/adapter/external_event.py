from datetime import datetime
from typing import Optional

from pydantic import BaseModel


class ExternalPatient(BaseModel):
    externalPatientId: str
    firstName: str
    lastName: str
    dateOfBirth: datetime
    gender: str


class ExternalPractitioner(BaseModel):
    externalPractitionerId: str
    firstName: str
    lastName: str
    specialty: Optional[str] = None
    email: Optional[str] = None


class ExternalEncounter(BaseModel):
    externalEncounterId: str
    encounterType: str
    startDateTime: datetime
    reason: Optional[str] = None
    location: Optional[str] = None


class ExternalHealthcareEvent(BaseModel):
    sourceSystem: str
    sourceSystemVersion: str
    externalReferenceId: str
    eventTimestamp: datetime

    patient: ExternalPatient
    encounter: ExternalEncounter
    practitioner: Optional[ExternalPractitioner] = None
