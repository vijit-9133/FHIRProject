from datetime import datetime
from app.adapter.external_event import (
    ExternalHealthcareEvent,
    ExternalPatient,
    ExternalEncounter,
    ExternalPractitioner,
)


def build_external_event(patient, encounter, practitioner):
    return ExternalHealthcareEvent(
        sourceSystem="ehr-simulator",
        sourceSystemVersion="1.0",
        externalReferenceId=encounter.uuid,
        eventTimestamp=datetime.utcnow(),
        patient=ExternalPatient(
            externalPatientId=patient.mrn,
            firstName=patient.given_name,
            lastName=patient.family_name,
            dateOfBirth=patient.birthdate,
            gender=patient.gender
        ),
        practitioner=ExternalPractitioner(
            externalPractitionerId=practitioner.provider_id,
            firstName=practitioner.given_name,
            lastName=practitioner.family_name,
            specialty=practitioner.specialty,
            email=practitioner.email
        ) if practitioner else None,
        encounter=ExternalEncounter(
            externalEncounterId=encounter.uuid,
            encounterType=encounter.encounter_type,
            startDateTime=encounter.start_datetime,
            reason=encounter.reason,
            location=encounter.location
        )
    )
