# Frontend - Admin & External System Management

## New Features Added

### 1. External System Flow (No Authentication Required)

#### External System Registration
- **Route**: `/external/register`
- **Purpose**: Allow external systems to self-register
- **Features**:
  - Input system name
  - Receive ClientId and ClientSecret (shown ONCE)
  - Status: PendingApproval
  - Warning to save ClientSecret securely

#### External System Status Check
- **Route**: `/external/status`
- **Purpose**: Check approval status using ClientId
- **Features**:
  - Input ClientId
  - View system information
  - Status indicators:
    - 🟡 PendingApproval → "Waiting for admin approval"
    - 🟢 Active → "Good to go"
    - 🔴 Suspended → "Access suspended"

### 2. Admin Flow (JWT Protected)

#### Admin Login
- **Route**: `/login`
- **Updated**: Added Admin role option
- **Quick Login**: Use "admin" button for demo
- **Redirects to**: `/admin/dashboard` after successful login

#### Admin Dashboard
- **Route**: `/admin/dashboard`
- **Protected by**: Admin Guard (JWT + Admin role required)
- **Features**:

##### External Systems Management Table
- View all registered external systems
- Columns:
  - System Name
  - Client ID
  - Status (color-coded badges)
  - Created At
  - Approved At
  - Last Accessed
  - Approved By
- Actions:
  - **Approve** (for PendingApproval systems)
  - **Suspend** (for Active systems)

##### Conversion Requests Overview
- Read-only table showing all conversion requests
- Columns:
  - ID
  - Resource Type
  - Status
  - Source System
  - Created At
  - Error Message (if failed)

##### Analytics Cards (Placeholder)
- Total Systems count
- Active Systems count
- Total Requests count

## Technical Implementation

### New Files Created

```
core/api/
├── external-system-api.service.ts  (External system APIs)
└── admin-api.service.ts            (Admin management APIs)

core/guards/
└── admin.guard.ts                  (Route protection)

features/conversion/pages/
├── external-register.component.ts  (Registration page)
└── external-status.component.ts    (Status check page)

features/dashboards/
└── admin-dashboard.component.ts    (Admin dashboard)

shared/models/
└── external-system.models.ts       (TypeScript interfaces)
```

### Updated Files

```
app.routes.ts                       (Added 3 new routes)
core/api/auth.models.ts            (Added Admin role)
core/api/auth.service.ts           (Handle Admin role)
features/auth/auth.models.ts       (Added Admin = 4 to enum)
features/auth/login.component.ts   (Added Admin login option)
```

### Routes Added

```typescript
/external/register      → External system registration (public)
/external/status        → External system status check (public)
/admin/dashboard        → Admin management dashboard (protected)
```

### API Endpoints Used

#### External System APIs (No JWT)
- `POST /api/admin/external-systems/register` - Register new system
- `GET /api/admin/external-systems` - Get all systems (for status lookup)

#### Admin APIs (JWT Required)
- `GET /api/admin/external-systems` - List all systems
- `POST /api/admin/external-systems/{id}/approve` - Approve system
- `POST /api/admin/external-systems/{id}/suspend` - Suspend system
- `GET /api/fhir/history` - Get conversion requests

## Usage Guide

### For External Systems

1. Navigate to `/external/register`
2. Enter your system name
3. **IMPORTANT**: Save the ClientSecret immediately (shown only once)
4. Note your ClientId
5. Wait for admin approval
6. Check status at `/external/status` using your ClientId
7. Once status is "Active", use ClientId and ClientSecret for API authentication

### For Admins

1. Navigate to `/login`
2. Select "Admin" role
3. Enter username (e.g., "admin")
4. Click Login or use quick login button
5. Access admin dashboard at `/admin/dashboard`
6. Manage external systems:
   - Approve pending systems
   - Suspend active systems
7. Monitor conversion requests

## Security

- External system routes are **public** (no JWT required)
- Admin dashboard is **protected** by:
  - JWT authentication
  - Admin role verification
  - Route guard (adminGuard)
- ClientSecret is:
  - Shown only once at registration
  - Never displayed again
  - Hashed on backend (SHA256)

## Styling

- Uses Bootstrap 5 classes
- Color-coded status badges:
  - 🟢 Green (bg-success) - Active/Completed
  - 🟡 Yellow (bg-warning) - PendingApproval/InProgress
  - 🔴 Red (bg-danger) - Suspended/Failed
- Responsive tables
- Card-based layouts

## Future Enhancements

- Analytics dashboard with charts
- System activity logs
- Bulk operations (approve/suspend multiple)
- Search and filter capabilities
- Export functionality
- Email notifications for approvals
