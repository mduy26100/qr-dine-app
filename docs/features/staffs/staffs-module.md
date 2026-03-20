# Staffs Module - Complete Documentation

The Staffs module manages restaurant staff members, enabling merchants to list and manage their team with pagination and search capabilities. Staff members are represented using the ApplicationUser domain model with the Staff role assignment.

---

## Overview

Staff Management provides a single read operation (paginated listing) that enables merchants to view all staff members within their organization with flexible filtering and pagination.

**Key Design:**

- Leverages ApplicationUser entity (shared with Identity module)
- Role-based identification via Staff role
- Merchant scoped (multi-tenant isolation)
- Expression-based DTOs (efficient database queries)

---

## CQRS Query

### GetStaffsPagedQuery

**File:** `src/QRDine.Application/Features/Staffs/Queries/GetStaffsPaged/`

Retrieves paginated, filterable list of staff members for a merchant.

**Request:**

```csharp
public class GetStaffsPagedQuery : PaginationRequest, IRequest<PagedResult<StaffDto>>
{
    public string? SearchTerm { get; set; }  // Optional search filter
    public int PageNumber { get; set; } = 1;  // Default: 1
    public int PageSize { get; set; } = 10;   // Default: 10, Max: 50
}
```

**Response:**

```csharp
public class PagedResult<StaffDto>
{
    public List<StaffDto> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}
```

**Handler Logic:**

1. Extract current user's `MerchantId` from `ICurrentUserService`
2. Validate MerchantId is not null (throws if null)
3. Call `IStaffService.GetStaffsPagedAsync(merchantId, query)`
4. Return `PagedResult<StaffDto>`

**Authentication:** Requires `Merchant` role  
**Authorization:** Automatically scoped to current user's merchant  
**Validation:**

- `PageNumber` > 0
- `PageSize` between 1-50

---

## Data Transfer Objects

### StaffDto

**File:** `src/QRDine.Application/Features/Staffs/DTOs/StaffDto.cs`

```csharp
public class StaffDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
}
```

**Purpose:** Represents a staff member in API responses  
**Source:** Projected from ApplicationUser entity via expression

---

## Infrastructure Layer

### IStaffService

**File:** `src/QRDine.Application/Features/Staffs/Services/IStaffService.cs`

```csharp
public interface IStaffService
{
    Task<PagedResult<StaffDto>> GetStaffsPagedAsync(
        Guid merchantId,
        GetStaffsPagedQuery request,
        CancellationToken cancellationToken
    );
}
```

### StaffService Implementation

**File:** `src/QRDine.Infrastructure/Staffs/Services/StaffService.cs`

**Responsibilities:**

1. Resolve Staff role ID from `Roles` table using `SystemRoles.Staff` constant
2. Build base query: Users with Staff role assignment
3. Create `GetStaffsPagedSpec` with filters and pagination
4. Execute dual-query via `SpecificationEvaluator`:
   - Count query (for total)
   - Result query (for paginated items)
5. Map to `PagedResult<StaffDto>`

**Query Pattern:**

- Retrieves staff from `Users` table
- Joins with `UserRoles` to find Staff role members
- Filters by `MerchantId`
- Applies search if provided
- Orders by `CreatedAt DESC`
- Paginates with offset/take

---

## Specifications

### GetStaffsPagedSpec

**File:** `src/QRDine.Infrastructure/Staffs/Specifications/GetStaffsPagedSpec.cs`

**Type:** `Specification<ApplicationUser, StaffDto>`

**Filter Chain:**

```
1. Tenant Isolation:
   Where(u => u.MerchantId == merchantId)

2. Search Filter (optional):
   If searchTerm provided:
     Where FirstName contains term OR
     Where LastName contains term OR
     Where Email contains term OR
     Where PhoneNumber contains term
   (case-insensitive substring matching)

3. Ordering:
   OrderByDescending(u => u.CreatedAt)

4. Pagination:
   Skip((pageNumber - 1) * pageSize)
   Take(pageSize)

5. Projection:
   Select(StaffExtensions.ToStaffDto)
```

**Caching:** None (real-time staff list)  
**Execution:** Via `SpecificationEvaluator`

---

## Extensions

### StaffExtensions

**File:** `src/QRDine.Infrastructure/Staffs/Extensions/StaffExtensions.cs`

Expression-based DTO projection:

```csharp
public static Expression<Func<ApplicationUser, StaffDto>> ToStaffDto =>
  u => new StaffDto
  {
    Id = u.Id,
    FirstName = u.FirstName ?? string.Empty,
    LastName = u.LastName ?? string.Empty,
    Email = u.Email ?? string.Empty,
    PhoneNumber = u.PhoneNumber,
    IsActive = u.IsActive
  };
```

**Purpose:** Enable efficient EF Core projections (minimal database transfer)

---

## API Endpoint

**Controller:** `src/QRDine.API/Controllers/Management/Staffs/StaffsController.cs`

**Route:** `[Route("api/v{version:apiVersion}/management/staffs")]`  
**API Version:** 1.0  
**Swagger Group:** Management  
**Authorization:** `[Authorize(Roles = SystemRoles.Merchant)]`

### GET /api/v1.0/management/staffs

**Method:** GET  
**Query Parameters:**

- `pageNumber` (int, default: 1) — Page number
- `pageSize` (int, default: 10, max: 50) — Items per page
- `searchTerm` (string?, optional) — Search filter

**Status Codes:**

- `200 OK` — Successfully retrieved staff list
- `401 Unauthorized` — Not authenticated
- `403 Forbidden` — Not Merchant role
- `400 Bad Request` — Invalid pagination params

**Response:**

```json
{
  "items": [ StaffDto[] ],
  "totalCount": 25,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

## Business Rules

✅ Merchants can only view their own staff members  
✅ Staff identified via ApplicationUser + Staff role  
✅ Search is case-insensitive, substring matching  
✅ PageSize limited to 50 maximum (performance)  
✅ Default ordering is by creation date (newest first)  
✅ Pagination uses offset-based model (page number + size)  
✅ IsActive flag tracks staff status  
✅ No staff creation/deletion in this module (handled in Identity)

---

## Multi-Tenancy

- **Isolation Level:** Query-based filtering by MerchantId
- **Enforcement:** Specification includes merchant filter
- **Additional Check:** ICurrentUserService validates MerchantId exists
- **Data Leakage:** Impossible - all queries filtered by merchant context

---

## Error Scenarios

| Scenario                           | Response                             | Status |
| ---------------------------------- | ------------------------------------ | ------ |
| User not authenticated             | Unauthorized error                   | 401    |
| User not Merchant role             | Forbidden error                      | 403    |
| MerchantId null (shouldn't happen) | BadRequest                           | 400    |
| Invalid pageNumber (< 1)           | BadRequest                           | 400    |
| Invalid pageSize (< 1 or > 50)     | BadRequest                           | 400    |
| SearchTerm with special SQL chars  | Handled safely (parameterized query) | 200    |
| No staff members found             | Empty items array, totalCount=0      | 200    |

---

## Example Workflows

### List All Staff (First Page)

```http
GET /api/v1.0/management/staffs
Authorization: Bearer <merchant_token>
```

Returns first 10 staff members sorted by newest first.

### Search Staff by Name

```http
GET /api/v1.0/management/staffs?searchTerm=john&pageSize=20
Authorization: Bearer <merchant_token>
```

Returns up to 20 staff members with "john" in name/email/phone.

### Paginate Through Large Team

```http
GET /api/v1.0/management/staffs?pageNumber=2&pageSize=50
Authorization: Bearer <merchant_token>
```

Returns second page of 50-item batches.

---

## Testing

Staff module should be tested for:

- ✓ Correct staff retrieval by merchant
- ✓ Search across all fields (FirstName, LastName, Email, PhoneNumber)
- ✓ Pagination accuracy (skip, take, totalCount)
- ✓ Multi-tenant isolation (merchant A cannot see merchant B's staff)
- ✓ Query performance with large staff lists
- ✓ Edge cases (empty search, no matches, max page size)

**Handler:** `src/QRDine.Application/Features/Identity/Commands/RegisterStaff/RegisterStaffCommandHandler.cs`

**Endpoint:** `POST /api/v1/users/register-staff`  
**Auth:** `Merchant` role required  
**Input:** `RegisterStaffDto`

```
{
  "email": "john@restaurant.com",
  "password": "Password123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+84912345678",
  "role": "Cashier"
}
```

**Output:** `RegisterResponseDto`

```
{
  "id": "staff-guid",
  "email": "john@restaurant.com",
  "firstName": "John",
  "lastName": "Doe",
  "merchantId": "merchant-guid"
}
```

**Business rules:**

1. Email must be globally unique
2. Staff automatically associated with current merchant
3. Password must meet ASP.NET Core Identity requirements
4. Role assignment happens at registration time
5. Staff user is created in `IsActive = true` state

---

## Staff Data Access

### Automatic Isolation via Global Query Filter

All staff queries automatically filtered to current merchant:

```csharp
// Staff user querying their own merchant's data
builder.Entity<ApplicationUser>()
    .HasQueryFilter(u => u.MerchantId == CurrentMerchantId);

// Result: Staff can only see other staff in their merchant
// Cross-tenant data access is impossible
```

### Ownership Check in Handlers

```csharp
public async Task<StaffResponseDto> Handle(UpdateStaffCommand request, CancellationToken ct)
{
    var staff = await _repository.GetByIdAsync(request.Id, ct);

    // Ownership check - staff must belong to current merchant
    if (staff.MerchantId != _currentUserService.MerchantId)
    {
        throw new ForbiddenException("Cannot modify staff from another merchant");
    }

    staff.FirstName = request.FirstName;
    await _repository.UpdateAsync(staff, ct);
    return _mapper.Map<StaffResponseDto>(staff);
}
```

---

## Staff Permissions Matrix

| Operation           | Merchant | Manager | Cashier | Kitchen | Waiter |
| ------------------- | -------- | ------- | ------- | ------- | ------ |
| View staff list     | ✅       | ✅      | ✅      | ✅      | ✅     |
| Add staff           | ✅       | ✅      | ❌      | ❌      | ❌     |
| Update staff        | ✅       | ✅      | ❌      | ❌      | ❌     |
| View orders         | ✅       | ✅      | ✅      | ✅      | ✅     |
| Update order status | ✅       | ✅      | ✅      | ✅      | ✅     |
| Process payment     | ✅       | ✅      | ✅      | ❌      | ❌     |
| View analytics      | ✅       | ✅      | ✅      | ❌      | ❌     |

---

## Staff Performance Metrics

### Tracked Metrics

- **Orders Processed** — Total orders handled by staff
- **Revenue Generated** — Total sales processed
- **Average Transaction Value** — Average order amount
- **Service Time** — Average time to complete order
- **Customer Ratings** — Average customer satisfaction

### Query: GetStaffPerformance

**Endpoint:** `GET /api/v1/management/staff/{id}/performance?from=2024-01-01&to=2024-01-31`  
**Auth:** `Merchant` role required  
**Output:** `StaffPerformanceDto`

```json
{
  "staffId": "guid",
  "name": "John Doe",
  "ordersProcessed": 342,
  "totalRevenue": 4200.5,
  "averageOrderValue": 12.28,
  "averageServiceTime": "3m 45s",
  "customerRating": 4.5
}
```

---

## Staff Deactivation

### Soft Deactivation

Instead of deletion, staff accounts are deactivated:

```csharp
public async Task DeactivateStaffAsync(Guid staffId, Guid merchantId)
{
    var staff = await _repository.GetByIdAsync(staffId);

    if (staff.MerchantId != merchantId)
        throw new ForbiddenException();

    staff.IsActive = false;
    await _repository.UpdateAsync(staff);
}
```

**Results:**

- Staff cannot log in (`IsActive = false`)
- Historical data is retained
- Performance metrics remain accessible
- Staff can be reactivated if needed

### Hard Termination

Merchants can permanently remove staff from system:

```csharp
await _repository.DeleteAsync(staffId);
```

---

## Staff Invitation & Onboarding (Future Enhancement)

**Planned workflow:**

1. Merchant creates staff invitation via `/api/v1/management/staff/invite`
2. System generates unique registration link
3. System sends invitation email to staff email address
4. Staff clicks link and completes registration
5. Confirmation email sent
6. Staff can now log in

---

## API Endpoints

| Method   | Path                                        | Auth     | Purpose                 |
| -------- | ------------------------------------------- | -------- | ----------------------- |
| `POST`   | `/api/v1/users/register-staff`              | Merchant | Register new staff      |
| `GET`    | `/api/v1/management/staff`                  | Merchant | List staff members      |
| `GET`    | `/api/v1/management/staff/{id}`             | Merchant | Get staff details       |
| `PUT`    | `/api/v1/management/staff/{id}`             | Merchant | Update staff info       |
| `PUT`    | `/api/v1/management/staff/{id}/role`        | Merchant | Change staff role       |
| `PATCH`  | `/api/v1/management/staff/{id}/deactivate`  | Merchant | Deactivate staff        |
| `DELETE` | `/api/v1/management/staff/{id}`             | Merchant | Delete staff (hard)     |
| `GET`    | `/api/v1/management/staff/{id}/performance` | Merchant | Get performance metrics |

---

## Integration with Other Modules

### Identity Module

- Staff registered via `RegisterStaffCommand`
- Uses ASP.NET Core Identity for auth
- Gets `Staff` role automatically

### Sales Module

- Staff can update order status
- Staff can view orders
- Staff actions tracked for performance

### Billing Module

- Staff count counts toward plan limit
- Manager role can view metrics
- Subscription limits enforce max staff

### Tenant Module

- Staff `MerchantId` enforces isolation
- Cannot access other merchants' data
- Global query filter applies transparently

---

## Security Considerations

1. **Password:** All passwords hashed by ASP.NET Core Identity
2. **Isolation:** Global query filter prevents cross-tenant access
3. **Audit:** Staff actions can be logged via middleware
4. **Rate Limiting:** API endpoints rate-limited per staff member
5. **Session:** JWT token includes `merchant_id` for authorization checks

---

**Reference:** See also [Staffs Module Overview](README.md), [Features Overview](../) for other modules, and [Identity Module](../identity/) for authentication details.
