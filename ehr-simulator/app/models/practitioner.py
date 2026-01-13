from pydantic import BaseModel


class Practitioner(BaseModel):
    provider_id: str
    given_name: str
    family_name: str
    specialty: str
    email: str
