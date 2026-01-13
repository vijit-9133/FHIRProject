from datetime import date
from typing import List, Optional

from pydantic import BaseModel


class OpenMRSIdentifier(BaseModel):
    identifier: str
    identifierType: str = "Medical Record Number"
    preferred: bool = True


class OpenMRSPersonName(BaseModel):
    givenName: str
    familyName: str
    preferred: bool = True


class OpenMRSPerson(BaseModel):
    preferredName: OpenMRSPersonName
    gender: str
    birthdate: date


class OpenMRSPatient(BaseModel):
    uuid: str
    identifiers: List[OpenMRSIdentifier]
    person: OpenMRSPerson
