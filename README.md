# FHIR Data Converter & Mapper

A clean, extensible **full-stack healthcare interoperability application** that converts custom (non-FHIR) healthcare data into **FHIR-compliant JSON resources**, enabling seamless data exchange between disparate healthcare systems.

This project demonstrates **real-world healthcare system design**, covering **FHIR standards**, **clean architecture**, **auditability**, and **modern Angular frontend development**, without unnecessary over-engineering.

---

## Why This Project Exists

Healthcare systems often:

- Store patient data in **custom, non-standard formats**
- Cannot easily exchange data across systems
- Struggle with **interoperability and compliance**

**FHIR (Fast Healthcare Interoperability Resources)** is the industry standard designed to solve this problem.

This project acts as a **conversion bridge**:

Custom Healthcare Data → Valid FHIR JSON → Persisted & Auditable Records

---

## Key Objectives

- Convert non-FHIR data into **real FHIR resources**
- Follow **Clean Architecture** principles
- Ensure **auditability and traceability**
- Design for **extensibility** (new FHIR resources)
- Build a **full-stack, production-ready system**
- Keep complexity intentional and readable

---

## Architecture Overview

This project follows a **modular monolith** using **Clean Architecture**:

Frontend (Angular)
↓
Controller → Service → Mapper / Validator → Repository → Database

### Design Principles

- Clear separation of concerns
- Async-first design
- Dependency inversion
- Thin controllers, rich services
- No microservices, queues, or CQRS overkill

---

## Technology Stack

### Backend
- **ASP.NET Core Web API (.NET 9)**
- **Entity Framework Core 9**
- **SQL Server**
- **HL7 FHIR .NET SDK (R4)**
- **System.Text.Json**
- **Swagger / OpenAPI**

### Frontend
- **Angular 18+**
- **Standalone Components (No NgModules)**
- **RxJS & HttpClient**
- **Reactive Forms**
- **Bootstrap (UI styling)**

---

## Core Backend Features

### Real FHIR Resource Generation
- Uses the **HL7 FHIR .NET SDK**
- Produces **valid FHIR R4 JSON**
- Avoids hand-crafted or “FHIR-like” models

### Resource-Specific Mappers
- Each FHIR resource has its own mapper
- Adding new resources requires **no service changes**
- Example: `PatientFhirMapper`

### Lightweight FHIR Validation
- Ensures required fields exist
- Returns structured validation errors
- Avoids heavy profile enforcement

### Explicit Audit Trail
Each conversion request follows a clear lifecycle:


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

## Frontend Application Overview

The frontend is a **modern Angular 18+ application** built using **standalone components** and **feature-based architecture**.

It provides a complete UI for:
- Submitting healthcare data for conversion
- Viewing conversion history
- Inspecting detailed FHIR output
- Auditing conversion requests

### Frontend Folder Structure

frontend/fhir-ui/src/app/
├── core/
│ ├── api/
│ │ ├── fhir-api.service.ts
│ │ └── api.models.ts
│ └── interceptors/
│ └── error.interceptor.ts
├── shared/
│ └── pipes/
│ └── pretty-json.pipe.ts
├── features/
│ ├── conversion/
│ ├── history/
│ └── details/
├── app.ts
├── app.config.ts
└── app.routes.ts

---

## Frontend Architecture Highlights

### Modern Angular Design
- **Standalone Components** (no NgModules)
- **Lazy-loaded feature routes**
- **Dependency Injection at root**
- **Reactive Forms with validation**
- **Strongly typed API contracts**

---

## Core Frontend Infrastructure

### FhirApiService
Centralized service for backend communication.

Features:
- HTTP client abstraction
- RxJS observable pattern
- Strong typing
- Integrated error handling

Supported APIs:
- Convert to FHIR
- Fetch conversion history
- Fetch conversion request
- Fetch generated FHIR resource

---

### Global Error Interceptor
- Catches all HTTP errors
- Logs errors for debugging
- Displays user-friendly messages
- Ensures consistent error handling

---

### Pretty JSON Pipe
- Formats JSON output
- Improves readability for FHIR inspection
- Used across details and conversion views

---

## Frontend Feature Modules

### Conversion Feature
- Reactive form for patient data
- Validation for required fields
- ISO date formatting for backend compatibility
- Displays generated FHIR JSON with tracking ID

### History Feature
- Lists all conversion requests
- Color-coded status badges:
  - 🟢 Success
  - 🟡 Pending
  - 🔴 Failed
- Responsive table design
- Navigation to detailed view

### Details Feature
- Displays full conversion context
- Raw input JSON
- Generated FHIR JSON
- Visual status indicators
- Responsive, card-based layout
- Back navigation for usability

---

## Conversion Flow (End-to-End)

1. User submits custom healthcare data
2. Conversion request created with **Pending** status
3. Resource-specific mapper generates FHIR JSON
4. FHIR resource is validated
5. Output persisted in database
6. Request marked **Success** or **Failed**
7. Structured response returned to UI
8. Conversion available in history & details view

---

## API Endpoints

POST /api/fhir/convert
GET /api/fhir/{conversionRequestId}
GET /api/fhir/request/{id}
GET /api/fhir/history

---

## Sample Request

```json
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
