# Staffs Module

Staff member management within merchant accounts.

## Quick Overview

The Staffs module manages restaurant staff members, enabling merchants to view, search, and manage their team members with pagination. Staff members are represented using the ApplicationUser model with the Staff role.

## Current Status

- ✅ Staff listing query implemented
- ✅ Pagination with configurable page size
- ✅ Multi-field search (name, email, phone)
- ✅ Merchant-scoped visibility (multi-tenant isolation)
- ✅ Staff status tracking

## Key Features

- ✅ Staff member listing with pagination
- ✅ Full-text search across first name, last name, email, phone number
- ✅ Merchant-scoped staff visibility (multi-tenant isolation)
- ✅ Staff status tracking (active/inactive)
- ✅ Staff profile information (name, email, phone)
- ✅ Creation date ordering (newest first)
- ✅ Configurable page sizes (up to 50 items per page)

## Data Model

Staff members use the **ApplicationUser** entity with the **Staff** role:

| Property      | Type       | Purpose                |
| ------------- | ---------- | ---------------------- |
| `Id`          | `Guid`     | Staff member ID        |
| `Email`       | `string`   | Email address          |
| `FirstName`   | `string?`  | First name             |
| `LastName`    | `string?`  | Last name              |
| `PhoneNumber` | `string?`  | Phone contact          |
| `MerchantId`  | `Guid?`    | Associated merchant    |
| `IsActive`    | `bool`     | Active/inactive status |
| `CreatedAt`   | `DateTime` | Staff creation date    |

## Use Cases

1. **Owner/Manager** views all staff members in their merchant account
2. **Owner** searches staff by name or email to find team member
3. **Owner** pages through large staff lists (50+ members)
4. **Owner** verifies staff contact information
5. **System** ensures staff can only access their own merchant's data
6. **Staff** logs in and sees their own profile in system

## API Endpoints

### Management API (Merchant Owner)

**Get Staff List** (Paginated & Searchable)

```http
GET /api/v1.0/management/staffs?pageNumber=1&pageSize=10&searchTerm=john
Auth: Merchant
```

**Query Parameters:**

- `pageNumber` - Page number (default: 1, min: 1)
- `pageSize` - Items per page (default: 10, max: 50)
- `searchTerm` - Optional search filter (searches: first name, last name, email, phone)

**Response (200 OK):**

```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@restaurant.com",
      "phoneNumber": "0912345678",
      "isActive": true
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "firstName": "Jane",
      "lastName": "Smith",
      "email": "jane@restaurant.com",
      "phoneNumber": "0987654321",
      "isActive": true
    }
  ],
  "totalCount": 15,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 2,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

## Features in Detail

### Pagination

- **Default page size:** 10 items
- **Maximum page size:** 50 items
- **Ordering:** By creation date (newest first)
- **Navigation metadata:**
  - `totalCount` - Total staff members
  - `totalPages` - Number of pages available
  - `hasPreviousPage` - Can fetch previous page
  - `hasNextPage` - Can fetch next page

### Search

Searches across 4 fields (case-insensitive):

1. First Name - e.g., "John"
2. Last Name - e.g., "Doe"
3. Email - e.g., "john@restaurant.com"
4. Phone Number - e.g., "0912345678"

Example: `searchTerm=john` matches:

- FirstName = "John" ✅
- LastName = "Johnson" ✅
- Email = "john@restaurant.com" ✅

### Multi-Tenancy

- Each staff member filtered by merchant ID
- Staff can only see their own merchant's team
- Enforcement at query level (query specification)
- Additional validation via current user context

## Documentation

- **[Complete Staffs Module Documentation](staffs-module.md)** — Full CQRS implementation details, DTOs, specifications, services

---

**See also:** [Features Overview](../) · [Identity Module](../identity/) · [Sales Module](../sales/)
