# Staffs Module - Complete Documentation

Staff member management within merchant accounts.

---

## Overview

The Staffs module enables merchants to register and manage staff members (employees) within their organization. Staff members have restricted access limited to their employer merchant, creating a clear organizational hierarchy.

---

## Staff Model

Staff members are represented by `ApplicationUser` with:

- `Role` = `Staff`
- `MerchantId` = Associated merchant's ID

| Property      | Type       | Purpose                 |
| ------------- | ---------- | ----------------------- |
| `Id`          | `Guid`     | Staff member identifier |
| `Email`       | `string`   | Unique email address    |
| `PhoneNumber` | `string?`  | Contact phone           |
| `FirstName`   | `string?`  | First name              |
| `LastName`    | `string?`  | Last name               |
| `AvatarUrl`   | `string?`  | Profile image URL       |
| `MerchantId`  | `Guid`     | Employer merchant ID    |
| `IsActive`    | `bool`     | Employment status       |
| `CreatedAt`   | `DateTime` | Hire date               |

---

## Staff Roles

Staff members are assigned role-based permissions within their merchant:

| Role         | Purpose                             | Permissions                                 |
| ------------ | ----------------------------------- | ------------------------------------------- |
| **Manager**  | Staff supervision, store operations | Full merchant access except billing         |
| **Cashier**  | Payment processing                  | Process orders, record payments, view sales |
| **Kitchen**  | Food preparation                    | View orders, update item status             |
| **Waiter**   | Customer service                    | Take orders, track delivery                 |
| **Delivery** | Order fulfillment                   | Manage deliveries (future feature)          |

---

## Staff Registration

### Command: RegisterStaff

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
