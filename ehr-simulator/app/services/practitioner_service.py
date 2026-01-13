from app.models.practitioner import Practitioner


class PractitionerService:
    _practitioners = []

    @classmethod
    def seed_demo_data(cls):
        if cls._practitioners:
            return

        cls._practitioners.append(
            Practitioner(
                provider_id="prov-1001",
                given_name="Gregory",
                family_name="House",
                specialty="Internal Medicine",
                email="house@example.com"
            )
        )

    @classmethod
    def get_primary_provider(cls):
        if not cls._practitioners:
            return None
        return cls._practitioners[0]
