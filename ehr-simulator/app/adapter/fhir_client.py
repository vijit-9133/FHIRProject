import requests

FHIR_PLATFORM_URL = "http://localhost:5078/api/integration/events"
CLIENT_ID = "external-system-1"
CLIENT_SECRET = "secret-key-1"


def send_to_fhir_platform(event_payload: dict):
    headers = {
        "Content-Type": "application/json",
        "X-Client-Id": CLIENT_ID,
        "X-Client-Secret": CLIENT_SECRET
    }

    response = requests.post(
        FHIR_PLATFORM_URL,
        json=event_payload,
        headers=headers,
        timeout=10
    )

    response.raise_for_status()
    return response.json()
