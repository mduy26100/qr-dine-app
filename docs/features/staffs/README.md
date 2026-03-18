# Staffs Module

Staff member management within merchant accounts.

## Quick Overview

The Staffs module enables merchants to register and manage staff members within their organization with role-based permissions.

## Key Features

- ✅ Staff registration by merchant owner
- ✅ Role assignment (Manager, Cashier, Kitchen, etc.)
- ✅ Limited permissions within merchant scope
- ✅ Staff performance tracking
- ✅ Access control per staff role

## Staff Roles

| Role        | Access                           | Responsibilities                    |
| ----------- | -------------------------------- | ----------------------------------- |
| **Manager** | Full access to merchant settings | Staff management, reports, strategy |
| **Cashier** | Order and payment management     | Ring up orders, process payments    |
| **Kitchen** | Order status updates             | Prepare items, update order status  |
| **Waiter**  | Table and order management       | Take orders, serve customers        |

## Entities

Uses `ApplicationUser` with `Staff` role and `MerchantId` to associate staff with merchant:

| Property     | Type      | Purpose             |
| ------------ | --------- | ------------------- |
| `Id`         | `Guid`    | Staff member ID     |
| `Email`      | `string`  | Email address       |
| `FirstName`  | `string?` | First name          |
| `LastName`   | `string?` | Last name           |
| `MerchantId` | `Guid`    | Associated merchant |
| `IsActive`   | `bool`    | Active status       |

## Use Cases

1. **Owner** opens merchant dashboard
2. **Owner** goes to Staff Management
3. **Owner** invites new staff via email
4. **Staff** receives invitation email
5. **Staff** clicks link and creates account
6. **Staff** logs in with their credentials
7. **Staff** can access merchant orders and tables (based on role)
8. **System** restricts access to other merchants' data

## Staff Registration

**Endpoint:** `POST /api/v1/users/register-staff`  
**Auth:** `Merchant` role required  
**Input:** `RegisterStaffDto` (Email, Password, FirstName, LastName, PhoneNumber, Role)  
**Output:** `RegisterResponseDto`

**Business rules:**

- Email must be unique within system
- Staff automatically associated with current merchant
- Staff cannot access other merchants' data
- Staff role determines permissions

## API Endpoints

| Method   | Path                                        | Auth     | Purpose                   |
| -------- | ------------------------------------------- | -------- | ------------------------- |
| `POST`   | `/api/v1/users/register-staff`              | Merchant | Register new staff        |
| `GET`    | `/api/v1/management/staff`                  | Merchant | List staff members        |
| `PUT`    | `/api/v1/management/staff/{id}`             | Merchant | Update staff              |
| `DELETE` | `/api/v1/management/staff/{id}`             | Merchant | Deactivate staff          |
| `GET`    | `/api/v1/management/staff/{id}/performance` | Merchant | Staff performance metrics |

## Permissions

Staff permissions are controlled by role plus MerchantId:

```csharp
// Staff can only access their merchant's data
var orders = await _orderRepository.ListAsync(
    new OrdersByMerchantSpec(_currentUser.MerchantId)
);

// Staff cannot see other merchants' data (global query filter enforces this)
```

## Staff Performance Metrics

Tracking staff performance for insights:

- Orders processed
- Average transaction value
- Customer satisfaction ratings
- Service time metrics

## Documentation

- **[Complete Staffs Module Documentation](staffs-module.md)** — Full documentation with all details

---

**Reference:** See also [Features Overview](../) for other modules and [Identity Module](../identity/) for user management.
