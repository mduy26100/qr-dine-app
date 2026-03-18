# Identity Module - Complete Documentation

The Identity module handles user authentication, registration, JWT token management, and role-based access control using ASP.NET Core Identity.

---

## System Roles

Defined in `src/QRDine.Infrastructure/Identity/Constants/SystemRoles.cs`:

| Role         | Description                                           | Registration Method                |
| ------------ | ----------------------------------------------------- | ---------------------------------- |
| `SuperAdmin` | Platform administrator. Can register merchants.       | Seeded on startup                  |
| `Merchant`   | Store owner. Manages their own catalog and staff.     | Registered by SuperAdmin           |
| `Staff`      | Store employee. Registered under a specific merchant. | Registered by Merchant             |
| `Guest`      | Public visitor.                                       | Not used in current implementation |

---

## Identity Models

### ApplicationUser

**File:** `src/QRDine.Infrastructure/Identity/Models/ApplicationUser.cs`

Extends `IdentityUser<Guid>` with custom properties:

| Property     | Type       | Description                                               |
| ------------ | ---------- | --------------------------------------------------------- |
| `FirstName`  | `string?`  | User's first name                                         |
| `LastName`   | `string?`  | User's last name                                          |
| `AvatarUrl`  | `string?`  | Profile image URL                                         |
| `FullName`   | `string`   | Computed: `$"{FirstName} {LastName}".Trim()` (not mapped) |
| `CreatedAt`  | `DateTime` | Account creation timestamp                                |
| `MerchantId` | `Guid?`    | Associated merchant (null for SuperAdmin)                 |
| `IsActive`   | `bool`     | Account active flag (default: true)                       |

### ApplicationRole

**File:** `src/QRDine.Infrastructure/Identity/Models/ApplicationRole.cs`

Extends `IdentityRole<Guid>` with an additional `Description` property.

### RefreshToken

**File:** `src/QRDine.Infrastructure/Identity/Models/RefreshToken.cs`

Stores refresh tokens for JWT renewal:

| Property      | Type       |
| ------------- | ---------- |
| `UserId`      | `Guid`     |
| `Token`       | `string`   |
| `ExpiresAt`   | `DateTime` |
| `CreatedByIp` | `string?`  |

### Permission & RolePermission

**Files:** `src/QRDine.Infrastructure/Identity/Models/Permission.cs`, `src/QRDine.Infrastructure/Identity/Models/RolePermission.cs`

Permission entities for future granular access control. Currently configured in EF Core but not actively used in CQRS handlers.

---

## CQRS Commands

### Login

**Command:** `src/QRDine.Application/Features/Identity/Commands/Login/LoginCommand.cs`  
**Handler:** `src/QRDine.Application/Features/Identity/Commands/Login/LoginCommandHandler.cs`

**Endpoint:** `POST /api/v1/auth/login`  
**Auth:** Public  
**Input:** `LoginRequestDto` (`Identifier`, `Password`)  
**Output:** `LoginResponseDto`

**Flow:**

1. Identifier can be **email** (contains `@`) or **phone number**.
2. Validates password via `UserManager.CheckPasswordAsync`.
3. Generates JWT access token with claims (NameIdentifier, Email, Name, Roles, MerchantId).
4. Generates a random refresh token and persists it to the database.
5. Returns access token, refresh token, expiry info, and user details.

**Response DTO (`LoginResponseDto`):**

| Field              | Type      | Description                                                                              |
| ------------------ | --------- | ---------------------------------------------------------------------------------------- |
| `AccessToken`      | `string`  | JWT access token                                                                         |
| `RefreshToken`     | `string`  | Refresh token string                                                                     |
| `ExpiresInMinutes` | `int`     | Access token TTL                                                                         |
| `User`             | `UserDto` | User profile (Id, Email, PhoneNumber, FirstName, LastName, AvatarUrl, Roles, MerchantId) |

### RegisterMerchant

**Command:** `src/QRDine.Application/Features/Identity/Commands/RegisterMerchant/RegisterMerchantCommand.cs`  
**Handler:** `src/QRDine.Application/Features/Identity/Commands/RegisterMerchant/RegisterMerchantCommandHandler.cs`

**Endpoint:** `POST /api/v1/users/register-merchant`  
**Auth:** `SuperAdmin` role required  
**Input:** `RegisterMerchantDto` (`Email`, `Password`, `FirstName`, `LastName`, `UserPhoneNumber`, `MerchantName`, `MerchantAddress`, `MerchantPhoneNumber`)  
**Output:** `RegisterResponseDto`

**Flow (transactional):**

1. Creates a new `Merchant` entity with a unique slug (auto-generated from `MerchantName`).
2. Creates an `ApplicationUser` with the `Merchant` role.
3. Associates the user with the newly created merchant.
4. Returns `MerchantId`, `MerchantName`, `FirstName`, `LastName`.

**Business rules:**

- Email uniqueness is enforced.
- Phone number uniqueness is enforced (if provided).
- Slug generation supports Vietnamese diacritics removal and URL-safe formatting.
- Duplicate slugs are resolved by appending a numeric suffix (e.g., `my-store-1`).

### RegisterStaff

**Command:** `src/QRDine.Application/Features/Identity/Commands/RegisterStaff/RegisterStaffCommand.cs`  
**Handler:** `src/QRDine.Application/Features/Identity/Commands/RegisterStaff/RegisterStaffCommandHandler.cs`

**Endpoint:** `POST /api/v1/users/register-staff`  
**Auth:** `Merchant` role required  
**Input:** `RegisterStaffDto` (`Email`, `Password`, `FirstName`, `LastName`, `PhoneNumber`)  
**Output:** `RegisterResponseDto`

**Flow:**

1. Reads the current user's `MerchantId` from JWT claims.
2. Creates an `ApplicationUser` with the `Staff` role, associated with the merchant.

---

## Services

### ILoginService / LoginService

**Interface:** `src/QRDine.Application/Features/Identity/Services/ILoginService.cs`  
**Implementation:** `src/QRDine.Infrastructure/Identity/Services/LoginService.cs`

Handles the complete authentication flow: credential validation, claim assembly, JWT generation, and refresh token storage.

### IRegisterService / RegisterService

**Interface:** `src/QRDine.Application/Features/Identity/Services/IRegisterService.cs`  
**Implementation:** `src/QRDine.Infrastructure/Identity/Services/RegisterService.cs`

Handles merchant and staff registration with:

- Duplicate email/phone validation
- Unique slug generation for merchants
- Transaction management for merchant registration

### IJwtTokenGenerator / JwtTokenGenerator

**Interface:** `src/QRDine.Application/Features/Identity/Services/IJwtTokenGenerator.cs`  
**Implementation:** `src/QRDine.Infrastructure/Identity/Services/JwtTokenGenerator.cs`

| Method                                | Purpose                                                                          |
| ------------------------------------- | -------------------------------------------------------------------------------- |
| `GenerateToken(claims)`               | Creates a signed JWT with HMAC-SHA256                                            |
| `GenerateRefreshToken()`              | Generates a cryptographically random 32-byte token                               |
| `GetRefreshTokenExpiration()`         | Returns expiry date based on `RefreshTokenExpiryDays` setting                    |
| `GetPrincipalFromExpiredToken(token)` | Validates an expired JWT (lifetime validation disabled) to extract the principal |

### ICurrentUserService / CurrentUserService

**Interface:** `src/QRDine.Application.Common/Abstractions/Identity/ICurrentUserService.cs`  
**Implementation:** `src/QRDine.Infrastructure/Identity/Services/CurrentUserService.cs`

Reads user context from `HttpContext.User` claims:

| Property          | Source Claim                               | Description              |
| ----------------- | ------------------------------------------ | ------------------------ |
| `UserId`          | `ClaimTypes.NameIdentifier`                | Current user's GUID      |
| `Roles`           | `ClaimTypes.Role`                          | List of assigned roles   |
| `MerchantId`      | `AppClaimTypes.MerchantId` (`merchant_id`) | Associated merchant GUID |
| `IsAuthenticated` | `Identity.IsAuthenticated`                 | Auth status              |

---

## Custom Claim Types

Defined in `src/QRDine.Infrastructure/Identity/Constants/AppClaimTypes.cs`:

```csharp
public static class AppClaimTypes
{
    public const string MerchantId = "merchant_id";
}
```

The `merchant_id` claim is embedded in the JWT during login and is the primary mechanism for multi-tenant context resolution.

---

## Data Seeding

On startup, `IdentitySeeder` (`src/QRDine.Infrastructure/Persistence/Seeding/IdentitySeeder.cs`) creates:

1. **All system roles**: `SuperAdmin`, `Merchant`, `Staff`, `Guest`
2. **Default SuperAdmin user**:
   - Username: `superadmin`
   - Email: `admin@qrdine.com`
   - Password: `Admin@123!`
   - `MerchantId`: `null` (platform-level user)

---

## Endpoint Summary

| Method | Path                              | Auth       | Description                                    |
| ------ | --------------------------------- | ---------- | ---------------------------------------------- |
| `POST` | `/api/v1/auth/login`              | Public     | Login with email/phone + password              |
| `POST` | `/api/v1/users/register-merchant` | SuperAdmin | Register a new merchant + owner account        |
| `POST` | `/api/v1/users/register-staff`    | Merchant   | Register a staff member under current merchant |

---

**Reference:** See also [Identity Module Overview](README.md) and [Features Overview](../) for other modules.
