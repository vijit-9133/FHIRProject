from datetime import datetime
from typing import Optional

from pydantic import BaseModel


class OpenMRSEncounter(BaseModel):
    uuid: str
    encounterType: str
    patient: dict
    encounterDatetime: datetime
    reason: Optional[str] = None
    location: Optional[str] = None
