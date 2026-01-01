# FHIR Data Converter & Mapper - Complete Project Overview

## Project Summary
A full-stack healthcare interoperability application that converts custom (non-FHIR) healthcare data into FHIR-compliant JSON resources. Built with ASP.NET Core Web API backend and Angular 20+ frontend, following Clean Architecture principles.

---

## Technology Stack

### Backend (.NET 9)
- **ASP.NET Core Web API 9.0**
- **Entity Framework Core 9.0** with SQL Server
- **HL7 FHIR .NET SDK (R4)** for official FHIR resource generation
- **System.Text.Json** for JSON serialization
- **Swagger/OpenAPI** for API documentation

### Frontend (Angular 20+)
- **Angular 20.3** with standalone components (no NgModules)
- **RxJS** for reactive programming
- **Reactive Forms** for form handling
- **Bootstrap** for UI styling
- **TypeScript 5.9**

---

## Architecture Overview

### Backend Architecture (Clean Architecture)
```
Controllers → Services → Mappers/Validators → Repositories → Database
```

**Key Design Principles:**
- Dependency Inversion
- Single Responsibility
- Async-first design
- Resource-specific mappers
- Centralized error handling
- Explicit audit trail

### Frontend Architecture (Feature-Based)
```
Core (API Services, Interceptors) → Features (Auth, Conversion, History, Details) → Shared Components
```

---

## Backend Implementation Details

### 1. Project Structure
```
FhirProject.Api/
├── Controllers/
│   ├── AuthController.cs
│   └── FhirConversionController.cs
├── Data/
│   └── AppDbContext.cs
├── DTOs/
│   ├── ConvertToFhirRequestDto.cs
│   ├── ConvertToFhirResponseDto.cs
│   ├── LoginRequestDto.cs
│   └── LoginResponseDto.cs
├── Mapping/
│   ├── IFhirResourceMapper.cs
│   ├── PatientFhirMapper.cs
│   ├── PractitionerFhirMapper.cs
│   ├── OrganizationFhirMapper.cs
│   └── UnsupportedResourceTypeException.cs
├── Middleware/
│   └── GlobalExceptionMiddleware.cs
├── Models/
│   ├── custom/ (Input models)
│   ├── entities/ (Database entities)
│   ├── enums/ (Enumerations)
│   └── fhir/ (FHIR models)
├── Repositories/
│   ├── Interfaces/
│   └── Implementations/
├── Services/
│   ├── Interfaces/
│   └── Implementations/
├── Validation/
│   ├── IFhirValidator.cs
│   ├── FhirPatientValidator.cs
│   ├── FhirPractitionerValidator.cs
│   ├── FhirOrganizationValidator.cs
│   └── FhirValidationException.cs
└── Migrations/ (EF Core migrations)
```

### 2. Core Features

#### FHIR Resource Mapping
- **PatientFhirMapper**: Converts custom patient data to FHIR Patient resource
- **PractitionerFhirMapper**: Converts custom practitioner data to FHIR Practitioner resource  
- **OrganizationFhirMapper**: Converts custom organization data to FHIR Organization resource
- Uses official HL7 FHIR .NET SDK for valid FHIR R4 JSON generation

#### Database Entities
- **ConversionRequestEntity**: Tracks conversion requests with audit trail
- **FhirResourceEntity**: Stores generated FHIR resources
- **UserEntity**: Manages user authentication and roles

#### Validation System
- Resource-specific validators ensure FHIR compliance
- Structured validation error responses
- Required field validation

#### Audit Trail
Each conversion follows this lifecycle:
1. Request created with `Pending` status
2. Resource-specific mapper generates FHIR JSON
3. FHIR resource validated
4. Output persisted in database
5. Request marked `Success` or `Failed`

### 3. API Endpoints
- `POST /api/fhir/convert` - Convert custom data to FHIR
- `GET /api/fhir/{conversionRequestId}` - Get FHIR resource by conversion ID
- `GET /api/fhir/request/{id}` - Get conversion request details
- `GET /api/fhir/history` - Get conversion history
- `POST /api/auth/login` - User authentication

### 4. Supported FHIR Resources
- **Patient** (FhirResourceType.Patient = 1)
- **Practitioner** (FhirResourceType.Practitioner = 2)
- **Organization** (FhirResourceType.Organization = 3)

---

## Frontend Implementation Details

### 1. Project Structure
```
src/app/
├── core/
│   ├── api/
│   │   ├── fhir-api.service.ts
│   │   ├── auth.service.ts
│   │   ├── api.models.ts
│   │   └── auth.models.ts
│   └── interceptors/
│       └── error.interceptor.ts
├── features/
│   ├── auth/
│   │   ├── login.component.ts
│   │   ├── auth-api.service.ts
│   │   └── auth.models.ts
│   ├── conversion/
│   │   ├── components/
│   │   │   └── conversion-form.component.ts
│   │   └── pages/
│   │       └── convert.component.ts
│   ├── dashboards/
│   │   ├── patient-dashboard.component.ts
│   │   ├── doctor-dashboard.component.ts
│   │   └── organization-dashboard.component.ts
│   ├── details/
│   │   └── pages/
│   │       └── details.component.ts
│   └── history/
│       └── pages/
│           └── history.component.ts
├── shared/
│   ├── components/ (Reusable UI components)
│   └── pipes/
│       └── pretty-json.pipe.ts
├── app.config.ts
├── app.routes.ts
└── app.ts
```

### 2. Core Features

#### Authentication System
- Role-based login (Patient, Doctor, Organization)
- JWT token management
- Route-based dashboards per user role

#### Conversion Feature
- Reactive forms for data input
- Real-time validation
- Support for all three FHIR resource types
- Display generated FHIR JSON with tracking ID

#### History Feature
- Lists all conversion requests
- Color-coded status badges (Success/Pending/Failed)
- Responsive table design
- Navigation to detailed view

#### Details Feature
- Complete conversion context display
- Raw input JSON and generated FHIR JSON
- Visual status indicators
- Responsive card-based layout

#### Shared Infrastructure
- **FhirApiService**: Centralized HTTP client with RxJS observables
- **ErrorInterceptor**: Global error handling
- **PrettyJsonPipe**: JSON formatting for display

### 3. Routing Configuration
```typescript
const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./features/auth/login.component') },
  { path: 'patient/dashboard', loadComponent: () => import('./features/dashboards/patient-dashboard.component') },
  { path: 'doctor/dashboard', loadComponent: () => import('./features/dashboards/doctor-dashboard.component') },
  { path: 'organization/dashboard', loadComponent: () => import('./features/dashboards/organization-dashboard.component') },
  { path: 'convert', loadComponent: () => import('./features/conversion/pages/convert.component') },
  { path: 'history', loadComponent: () => import('./features/history/pages/history.component') },
  { path: 'details/:id', loadComponent: () => import('./features/details/pages/details.component') }
];
```

---

## Database Schema

### ConversionRequestEntity
- `Id` (Guid) - Primary key
- `ResourceType` (FhirResourceType enum)
- `InputData` (JSON string)
- `Status` (ConversionStatus enum)
- `ErrorMessage` (string, nullable)
- `CreatedAt` (DateTime)
- `UpdatedAt` (DateTime)
- `UserId` (Guid, foreign key)

### FhirResourceEntity
- `Id` (Guid) - Primary key
- `ConversionRequestId` (Guid, foreign key)
- `FhirJson` (JSON string)
- `ResourceType` (string)
- `CreatedAt` (DateTime)

### UserEntity
- `Id` (Guid) - Primary key
- `Username` (string)
- `PasswordHash` (string)
- `Role` (UserRole enum)
- `CreatedAt` (DateTime)

---

## Sample Data Flow

### 1. Input Request
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
```

### 2. Generated FHIR Output
```json
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
  "birthDate": "1990-05-14",
  "telecom": [
    {
      "system": "phone",
      "value": "+1-555-123-4567"
    },
    {
      "system": "email",
      "value": "john.doe@example.com"
    }
  ],
  "address": [
    {
      "line": ["123 Main Street"],
      "city": "San Francisco",
      "state": "CA",
      "postalCode": "94105",
      "country": "USA"
    }
  ]
}
```

---

## Key Implementation Highlights

### Backend
1. **Clean Architecture**: Clear separation of concerns with dependency inversion
2. **Resource-Specific Mappers**: Each FHIR resource has dedicated mapper - easily extensible
3. **Official FHIR SDK**: Uses HL7 FHIR .NET SDK for valid FHIR R4 compliance
4. **Comprehensive Validation**: Resource-specific validators with structured error responses
5. **Audit Trail**: Complete conversion lifecycle tracking with status management
6. **Global Exception Handling**: Centralized error handling with consistent API responses

### Frontend
1. **Modern Angular**: Standalone components, lazy loading, reactive forms
2. **Feature-Based Architecture**: Organized by business features, not technical layers
3. **Strongly Typed**: Full TypeScript integration with API contracts
4. **Reactive Programming**: RxJS observables throughout
5. **Role-Based UI**: Different dashboards per user role
6. **Responsive Design**: Bootstrap-based responsive UI

---

## Development Setup

### Backend
1. **Prerequisites**: .NET 9 SDK, SQL Server
2. **Database**: Entity Framework migrations included
3. **Configuration**: Connection string in appsettings.json
4. **Run**: `dotnet run` (launches on http://localhost:5078)

### Frontend
1. **Prerequisites**: Node.js 18+, Angular CLI
2. **Install**: `npm install`
3. **Run**: `ng serve` (launches on http://localhost:4200)

---

## Production Considerations

### Backend
- SQL Server database with proper connection pooling
- Global exception middleware prevents stack trace leakage
- CORS configured for cross-origin requests
- Swagger documentation available in development

### Frontend
- Lazy-loaded routes for optimal performance
- Error interceptor for consistent error handling
- Production build optimization with Angular CLI
- Environment-specific configurations

---

This project demonstrates real-world healthcare system design with FHIR standards compliance, clean architecture principles, and modern full-stack development practices.
