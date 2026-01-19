# Frontend Architecture Summary

## Component Hierarchy

```
App Root
│
├── Login Component (Updated)
│   ├── Patient Login
│   ├── Practitioner Login
│   ├── Organization Login
│   └── Admin Login ← NEW
│
├── External System Flow (No Auth Required)
│   ├── External Register Component ← NEW
│   │   └── /external/register
│   │       ├── Input: System Name
│   │       ├── Output: ClientId + ClientSecret (once)
│   │       └── Status: PendingApproval
│   │
│   └── External Status Component ← NEW
│       └── /external/status
│           ├── Input: ClientId
│           └── Output: System Info + Status
│
├── Admin Flow (JWT Protected)
│   └── Admin Dashboard Component ← NEW
│       └── /admin/dashboard (protected by adminGuard)
│           ├── External Systems Management
│           │   ├── List all systems
│           │   ├── Approve pending systems
│           │   └── Suspend active systems
│           ├── Conversion Requests Overview
│           │   └── Read-only request history
│           └── Analytics Cards
│               ├── Total Systems
│               ├── Active Systems
│               └── Total Requests
│
└── Existing Features (Unchanged)
    ├── Patient Dashboard
    ├── Doctor Dashboard
    ├── Organization Dashboard
    ├── Conversion Pages
    ├── History Pages
    └── Details Pages
```

## Service Layer

```
Core Services
│
├── AuthService (Updated)
│   ├── login() - handles Admin role
│   ├── logout()
│   ├── getCurrentUser()
│   ├── isLoggedIn()
│   └── hasRole() - checks Admin role
│
├── ExternalSystemApiService ← NEW
│   ├── register() - POST /api/admin/external-systems/register
│   └── getAllSystems() - GET /api/admin/external-systems
│
└── AdminApiService ← NEW
    ├── getAllSystems() - GET /api/admin/external-systems
    ├── approveSystem() - POST /api/admin/external-systems/{id}/approve
    ├── suspendSystem() - POST /api/admin/external-systems/{id}/suspend
    └── getConversionHistory() - GET /api/fhir/history
```

## Route Guards

```
Guards
│
├── adminGuard ← NEW
│   ├── Checks: isLoggedIn() && hasRole('Admin')
│   ├── Protects: /admin/dashboard
│   └── Redirects to: /login if unauthorized
│
└── (Other guards - if any)
```

## Data Flow

### External System Registration Flow
```
User (External System)
    ↓
[External Register Component]
    ↓
ExternalSystemApiService.register()
    ↓
POST /api/admin/external-systems/register
    ↓
Backend creates system with PendingApproval
    ↓
Response: { clientId, clientSecret, status }
    ↓
Display credentials (ClientSecret shown ONCE)
```

### External System Status Check Flow
```
User (External System)
    ↓
[External Status Component]
    ↓
Input: ClientId
    ↓
ExternalSystemApiService.getAllSystems()
    ↓
GET /api/admin/external-systems
    ↓
Filter client-side by ClientId
    ↓
Display: System info + status badge
```

### Admin Approval Flow
```
Admin User
    ↓
[Login Component] - Select Admin role
    ↓
AuthService.login()
    ↓
POST /api/auth/login
    ↓
JWT stored in localStorage
    ↓
Navigate to /admin/dashboard
    ↓
adminGuard checks role
    ↓
[Admin Dashboard Component]
    ↓
AdminApiService.getAllSystems()
    ↓
Display systems table
    ↓
Admin clicks "Approve"
    ↓
AdminApiService.approveSystem(id)
    ↓
POST /api/admin/external-systems/{id}/approve
    ↓
Backend updates status to Active
    ↓
Refresh systems list
    ↓
Status badge changes to green
```

## State Management

### Authentication State
```typescript
// Stored in localStorage
{
  userId: number,
  role: 'Patient' | 'Practitioner' | 'Organization' | 'Admin'
}

// Managed by AuthService
currentUser$: Observable<User | null>
```

### Component State (Local)
```typescript
// External Register Component
{
  systemName: string,
  loading: boolean,
  errorMessage: string,
  registrationResponse: ExternalSystemRegistrationResponse | null
}

// External Status Component
{
  clientId: string,
  loading: boolean,
  errorMessage: string,
  system: ExternalSystemDto | null
}

// Admin Dashboard Component
{
  systems: ExternalSystemDto[],
  requests: ConversionRequestDto[],
  loadingSystems: boolean,
  loadingRequests: boolean,
  actionLoading: boolean,
  loadingError: string
}
```

## API Models

```typescript
// External System Registration
interface ExternalSystemRegistrationRequest {
  systemName: string;
}

interface ExternalSystemRegistrationResponse {
  id: number;
  clientId: string;
  clientSecret: string;  // Shown only once
  systemName: string;
  status: string;
  createdAt: string;
}

// External System DTO
interface ExternalSystemDto {
  id: number;
  clientId: string;
  systemName: string;
  status: 'PendingApproval' | 'Active' | 'Suspended';
  createdAt: string;
  approvedAt?: string;
  lastAccessedAt?: string;
  approvedByUser?: {
    userId: number;
    username: string;
  };
}

// Conversion Request DTO
interface ConversionRequestDto {
  id: number;
  resourceType: string;
  status: string;
  sourceSystem?: string;
  createdAt: string;
  errorMessage?: string;
}
```

## Styling & UI Components

### Bootstrap Classes Used
- `container`, `container-fluid` - Layout
- `card`, `card-header`, `card-body` - Card components
- `table`, `table-striped`, `table-hover` - Tables
- `btn`, `btn-primary`, `btn-success`, `btn-danger` - Buttons
- `badge`, `bg-success`, `bg-warning`, `bg-danger` - Status badges
- `alert`, `alert-danger`, `alert-success`, `alert-info` - Alerts
- `form-control`, `form-label` - Forms
- `spinner-border` - Loading indicators

### Status Badge Colors
- 🟢 **Green** (bg-success) - Active, Completed, Success
- 🟡 **Yellow** (bg-warning) - PendingApproval, InProgress, Pending
- 🔴 **Red** (bg-danger) - Suspended, Failed, Error
- ⚫ **Gray** (bg-secondary) - Unknown, Inactive

## Security Implementation

### Route Protection
```typescript
// app.routes.ts
{
  path: 'admin/dashboard',
  loadComponent: () => import('./features/dashboards/admin-dashboard.component'),
  canActivate: [adminGuard]  // ← Protects route
}
```

### Guard Implementation
```typescript
// admin.guard.ts
export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn() && authService.hasRole('Admin')) {
    return true;  // Allow access
  }

  router.navigate(['/login']);  // Redirect
  return false;  // Deny access
};
```

### JWT Handling
- Stored in localStorage by AuthService
- Automatically attached to requests by auth.interceptor
- Validated on backend for admin endpoints
- Expires after configured time (backend setting)

## Separation of Concerns

### External System UI
- **No authentication required**
- **No JWT tokens**
- **No data access**
- **Purpose**: Registration + status check only
- **Routes**: `/external/*`

### Admin UI
- **JWT authentication required**
- **Admin role required**
- **Full management access**
- **Purpose**: System approval + monitoring
- **Routes**: `/admin/*`

### Existing User UI
- **JWT authentication required**
- **Role-based access** (Patient/Practitioner/Organization)
- **Data conversion access**
- **Purpose**: FHIR conversion workflows
- **Routes**: `/patient/*`, `/doctor/*`, `/organization/*`, `/conversion/*`, `/history/*`

## Integration Points

### With Existing Auth System
- Reuses AuthService
- Extends User role type
- Uses existing auth.interceptor
- Shares JWT infrastructure

### With Backend APIs
- External system registration (public)
- External system status (public)
- Admin management (protected)
- Conversion history (protected)

### With Existing Components
- Shares Bootstrap styling
- Uses same routing patterns
- Follows same component structure
- Maintains consistency

## Future Extension Points

1. **Analytics Dashboard**
   - Add chart library (Chart.js, ngx-charts)
   - Create analytics service
   - Add time-series data endpoints

2. **System Activity Logs**
   - Create activity log component
   - Add activity log API service
   - Display in admin dashboard

3. **Notifications**
   - Email notifications for approvals
   - In-app notifications
   - WebSocket for real-time updates

4. **Bulk Operations**
   - Multi-select in table
   - Bulk approve/suspend
   - Export to CSV

5. **Search & Filter**
   - Search by system name
   - Filter by status
   - Date range filters
