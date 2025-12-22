# FHIR Data Converter & Mapper

A clean, extensible ASP.NET Core Web API that converts custom (non-FHIR) healthcare data into FHIR-compliant JSON resources, enabling interoperability between disparate healthcare systems.

This project demonstrates real-world healthcare backend design, including FHIR standards, clean architecture, auditability, and structured validation, without unnecessary complexity.

---

## Why This Project Exists

Healthcare systems often:
- Store patient data in custom, non-standard formats
- Cannot easily exchange data with other systems
- Struggle with interoperability and compliance

FHIR (Fast Healthcare Interoperability Resources) is the industry standard for solving this problem.

This project acts as a conversion bridge:

Custom Healthcare Data -> Valid FHIR JSON -> Persisted and Auditable

---

## Key Objectives

- Convert non-FHIR data into real FHIR resources
- Follow Clean Architecture principles
- Ensure auditability and traceability
- Design for extensibility (new FHIR resources)
- Keep the system simple, readable, and production-ready

---

## Architecture Overview

This project follows a modular monolith using Clean Architecture:

Controller -> Service -> Mapper / Validator -> Repository -> Database

Design principles:
- Clear separation of concerns
- Async-first design
- Dependency inversion
- No overengineering (no microservices, queues, or CQRS)

---

## Technology Stack

Backend: ASP.NET Core Web API (.NET 9)  
Database: SQL Server  
ORM: Entity Framework Core 9  
FHIR: HL7 FHIR .NET SDK (R4)  
Serialization: System.Text.Json  
API Documentation: Swagger / OpenAPI

---

## Core Features

### Real FHIR Resource Generation
- Uses the HL7 FHIR .NET SDK
- Produces valid FHIR R4 JSON
- Avoids hand-crafted or FHIR-like models

### Resource-Specific Mappers
- Each FHIR resource has its own mapper
- Adding new resources requires no service changes
- Example: PatientFhirMapper

### Lightweight FHIR Validation
- Validates generated FHIR resources
- Ensures required fields are present
- Returns structured validation errors
- Avoids heavy profile enforcement

### Explicit Audit Trail
Each conversion request has a clear lifecycle:

Pending -> Success | Failed

Stored metadata includes:
- Conversion status
- Error message (on failure)
- Mapping version
- Raw input JSON
- Generated FHIR JSON

### Centralized Error Handling
- Global exception middleware
- Consistent API error responses
- No stack trace leakage
- Proper HTTP status mapping

---

## Project Structure
Controllers
Services
Mapping
IFhirResourceMapper
PatientFhirMapper
Validation
IFhirValidator
FhirPatientValidator
Repositories
Middleware
GlobalExceptionMiddleware
Entities
DTOs
Enums
Data

---

## Conversion Flow

1. Client submits custom healthcare data
2. Conversion request is created with status Pending
3. Resource-specific mapper generates FHIR JSON
4. FHIR resource is validated
5. FHIR JSON is persisted
6. Conversion request updated to Success or Failed
7. Structured response returned to the client

---

## API Endpoints

POST /api/fhir/convert  
GET  /api/fhir/{conversionRequestId}  
GET  /api/fhir/request/{id}  
GET  /api/fhir/history  

---

## Sample Request

{
  "resourceType": 1,
  "data": {
    "firstName": "John",
    "lastName": "Doe",
    "dateOfBirth": "1990-05-14T00:00:00Z",
    "gender": "male",
    "phoneNumber": "+1-555-123-4567",
    "email": "john.doe@example.com",
    "address": {
      "line1": "123 Main Street",
      "city": "San Francisco",
      "state": "CA",
      "postalCode": "94105",
      "country": "USA"
    }
  }
}
# Sample Response
{
  "resourceType": "Patient",
  "name": [
    {
      "use": "official",
      "family": "Doe",
      "given": ["John"]
    }
  ],
  "gender": "male",
  "birthDate": "1990-05-14"
}


