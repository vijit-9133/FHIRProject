import uuid
from datetime import datetime
from typing import Optional

from pydantic import BaseModel


class EncounterModel(BaseModel):
    uuid: str
    patient_uuid: str
    encounter_type: str
    reason: Optional[str] = None
    start_datetime: datetime
    end_datetime: Optional[datetime] = None
    location: Optional[str] = None

    @staticmethod
    def create(
        patient_uuid: str,
        encounter_type: str,
        reason: Optional[str] = None,
        location: Optional[str] = None
    ) -> "EncounterModel":
        return EncounterModel(
            uuid=str(uuid.uuid4()),
            patient_uuid=patient_uuid,
            encounter_type=encounter_type,
            reason=reason,
            start_datetime=datetime.utcnow(),
            location=location
        )
