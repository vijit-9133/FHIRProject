from fastapi import APIRouter
from app.services.practitioner_service import PractitionerService

router = APIRouter(prefix="/ws/rest/v1/provider", tags=["OpenMRS Provider"])


@router.get("")
def get_providers():
    PractitionerService.seed_demo_data()
    provider = PractitionerService.get_primary_provider()

    return [{
        "uuid": provider.uuid,
        "identifier": provider.provider_id,
        "person": {
            "display": f"{provider.given_name} {provider.family_name}"
        },
        "attributes": {
            "specialty": provider.specialty,
            "email": provider.email
        }
    }]
