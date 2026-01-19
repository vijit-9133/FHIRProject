# Quick Start Guide - Testing Admin & External System Features

## Prerequisites
- Backend running on `http://localhost:5078`
- Frontend running on Angular dev server
- Admin user exists in database (username: "admin")

## Test Scenario 1: External System Registration

1. **Open browser**: Navigate to `http://localhost:4200/external/register`

2. **Register a system**:
   - Enter System Name: "Hospital Management System"
   - Click "Register"

3. **Save credentials** (IMPORTANT):
   ```
   ClientId: ext-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
   ClientSecret: [32-character random string]
   Status: PendingApproval
   ```
   ⚠️ Copy and save the ClientSecret - it will never be shown again!

4. **Check status**:
   - Click "Check Status" button or navigate to `/external/status`
   - Enter your ClientId
   - Should see: "⏳ Waiting for admin approval"

## Test Scenario 2: Admin Approval Flow

1. **Admin Login**:
   - Navigate to `http://localhost:4200/login`
   - Select "Admin" role
   - Enter username: "admin"
   - Click "Login" or use quick login "admin" button

2. **Access Dashboard**:
   - Should automatically redirect to `/admin/dashboard`
   - See "Admin Dashboard" page

3. **View External Systems**:
   - See table with registered system
   - Status should be "PendingApproval" (yellow badge)

4. **Approve System**:
   - Click "Approve" button
   - System status changes to "Active" (green badge)
   - "Approved At" timestamp populated

5. **Verify Approval**:
   - Open new incognito window
   - Navigate to `/external/status`
   - Enter the ClientId
   - Should now see: "✓ Good to go" (green)

## Test Scenario 3: System Suspension

1. **As Admin** (in `/admin/dashboard`):
   - Find the Active system
   - Click "Suspend" button
   - Confirm suspension

2. **Verify Suspension**:
   - Status changes to "Suspended" (red badge)
   - Check status page shows: "⛔ Access suspended"

## Test Scenario 4: Multiple Systems

1. **Register multiple systems**:
   - System 1: "Hospital A"
   - System 2: "Clinic B"
   - System 3: "Lab C"

2. **Admin Dashboard**:
   - See all 3 systems in table
   - Approve some, leave others pending
   - Suspend one active system

3. **Analytics Cards**:
   - Total Systems: 3
   - Active Systems: [count of approved systems]
   - Total Requests: [from conversion history]

## Test Scenario 5: Conversion Requests Overview

1. **As Admin** (in `/admin/dashboard`):
   - Scroll to "Conversion Requests Overview" section
   - View all conversion requests from all systems
   - See status, source system, timestamps

## API Testing with Postman

### After System is Approved:

1. **Get JWT Token**:
```http
POST http://localhost:5078/api/auth/external/token
Content-Type: application/json

{
  "clientId": "ext-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "clientSecret": "[your-saved-secret]"
}
```

Response:
```json
{
  "accessToken": "eyJhbGc...",
  "expiresIn": 3600
}
```

2. **Use Token for API Calls**:
```http
GET http://localhost:5078/api/external/events
Authorization: Bearer eyJhbGc...
```

## Expected Behaviors

### External System Pages (No Auth)
- ✅ Can access `/external/register` without login
- ✅ Can access `/external/status` without login
- ✅ ClientSecret shown only once
- ✅ Status updates reflect immediately

### Admin Dashboard (Auth Required)
- ✅ Redirects to `/login` if not authenticated
- ✅ Redirects to `/login` if not Admin role
- ✅ Can approve pending systems
- ✅ Can suspend active systems
- ✅ Cannot approve already active systems
- ✅ Cannot suspend already suspended systems

### Security
- ❌ Non-admin users cannot access `/admin/dashboard`
- ❌ Suspended systems cannot get JWT tokens
- ❌ Invalid ClientId/ClientSecret rejected
- ✅ JWT expires after 1 hour

## Troubleshooting

### "Failed to load external systems"
- Check backend is running
- Check API endpoint: `http://localhost:5078/api/admin/external-systems`
- Check browser console for CORS errors

### "Redirected to login when accessing admin dashboard"
- Ensure you logged in as Admin role
- Check localStorage for currentUser
- Verify JWT token in localStorage

### "System not found with this Client ID"
- Verify ClientId is correct (copy-paste)
- Check system was actually registered
- Try refreshing the page

### "Failed to approve/suspend system"
- Check JWT token is valid
- Verify Admin role in token
- Check backend logs for authorization errors

## Demo Credentials

### Human Users (Existing)
- Patient: `patient1` (role: Patient)
- Doctor: `doctor1` (role: Practitioner)
- Hospital: `hospital1` (role: Organization)
- **Admin: `admin` (role: Admin)** ← NEW

### External Systems
- Register via `/external/register`
- No predefined credentials
- Each registration generates unique ClientId and ClientSecret

## Next Steps

After testing:
1. Integrate with existing conversion flow
2. Add system activity logging
3. Implement analytics charts
4. Add email notifications
5. Create system API usage dashboard
