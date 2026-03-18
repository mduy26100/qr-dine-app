# Authentication

Complete guide to JWT authentication, token lifecycle, and login flow in QRDine.

## JWT Bearer Tokens

QRDine uses stateless JWT (JSON Web Token) authentication for all protected endpoints.

### Token Structure

```json
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "email": "merchant@example.com",
  "merchant_id": "550e8400-e29b-41d4-a716-446655440001",
  "role": "Merchant",
  "iat": 1708770896,
  "exp": 1708771496,
  "iss": "https://qrdine.com",
  "aud": "https://qrdine.com"
}
```

**Claims:**

| Claim         | Description                | Example                           |
| ------------- | -------------------------- | --------------------------------- |
| `sub`         | Subject (User ID)          | UUID of authenticated user        |
| `email`       | User email                 | `merchant@example.com`            |
| `merchant_id` | Associated merchant/tenant | UUID of merchant                  |
| `role`        | User role                  | `Merchant`, `Staff`, `SuperAdmin` |
| `iat`         | Issued at timestamp        | Unix timestamp (seconds)          |
| `exp`         | Expiration timestamp       | Unix timestamp (seconds)          |
| `iss`         | Issuer                     | `https://qrdine.com`              |
| `aud`         | Audience                   | `https://qrdine.com`              |

### Token Lifetime

**Access Token:**

- Default: 10 minutes
- Location: Memory
- Revocation: Not revoked (stateless)
- Use case: API requests

**Refresh Token:**

- Default: 7 days
- Location: HTTP-only cookie OR response body
- Revocation: Tracked in database
- Use case: Obtain new access token

## Login Flow

### Step 1: Send Credentials

**Request:**

```bash
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "merchant@example.com",
  "password": "SecurePassword!123"
}
```

### Step 2: Validate Credentials

Server validates:

1. User exists with provided email
2. Provided password matches stored hash (PBKDF2)
3. User account is active/not locked

Failure → HTTP 401 Unauthorized

### Step 3: Generate Tokens

On successful validation:

```csharp
var accessToken = new JwtSecurityToken(
    issuer: _settings.ValidIssuer,
    audience: _settings.ValidAudience,
    claims: new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(AppClaimTypes.MerchantId, user.MerchantId.ToString()),
        new Claim(ClaimTypes.Role, role.Name)
    },
    expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
    signingCredentials: new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret)),
        SecurityAlgorithms.HmacSha256)
);

var refreshToken = Guid.NewGuid().ToString("N");
```

### Step 4: Return Tokens

**Response (200 OK):**

```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1NTBlODQwMC1lMjliLTQxZDQtYTcxNi00NDY2NTU0NDAwMDAiLCJlbWFpbCI6Im1lcmNoYW50QGV4YW1wbGUuY29tIiwibWVyY2hhbnRfaWQiOiI1NTBlODQwMC1lMjliLTQxZDQtYTcxNi00NDY2NTU0NDAwMDEiLCJyb2xlIjoiTWVyY2hhbnQiLCJpYXQiOjE3MDg3NzA4OTYsImV4cCI6MTcwODc3MTQ5NiwiaXNzIjoiaHR0cHM6Ly9xcmRpbmUuY29tIiwiYXVkIjoiaHR0cHM6Ly9xcmRpbmUuY29tIn0.abcd1234...",
    "refreshToken": "550e8400e29b41d4a716446655440002",
    "expiresIn": 600,
    "tokenType": "Bearer"
  },
  "meta": {
    /* ... */
  }
}
```

**Fields:**

- `accessToken` — JWT for API requests (valid 10 minutes)
- `refreshToken` — UUID for token refresh (valid 7 days)
- `expiresIn` — Access token lifetime in seconds
- `tokenType` — Always "Bearer"

## Using Access Token

Include in `Authorization` header for protected endpoints:

```bash
GET /api/v1/management/products
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Token Validation:**

Server validates:

1. Token signature matches secret key
2. Token not expired (`exp` > current time)
3. Issuer and audience match configuration
4. Token claims present and valid

Invalid token → HTTP 401 Unauthorized

## Refresh Token Flow

When access token expires (10 minutes), use refresh token to obtain new access token.

### Step 1: Send Refresh Token

```bash
POST /api/v1/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "550e8400e29b41d4a716446655440002"
}
```

### Step 2: Validate Refresh Token

Server checks:

1. Refresh token exists in database
2. Token not expired (`expiryDate` > current time)
3. Token not revoked (`isRevoked` = false)
4. Associated user still active

Failure → HTTP 403 Forbidden "Refresh token invalid or expired"

### Step 3: Generate New Tokens

Issue new access token + refresh token.

### Step 4: Revoke Old Token

```csharp
tokenEntity.IsRevoked = true;
await _db.SaveChangesAsync();
```

Prevents token replay attacks and enables token invalidation.

### Step 5: Return New Tokens

**Response (200 OK):**

```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "550e8400e29b41d4a716446655440003",
    "expiresIn": 600,
    "tokenType": "Bearer"
  },
  "meta": {
    /* ... */
  }
}
```

## Token Rotation Strategy

Refresh token rotation prevents replay attacks:

```
1. Client stores access_token + refresh_token
2. Client uses access_token for API requests
3. After 10 minutes, access_token expires (401)
4. Client sends refresh_token to /auth/refresh-token
5. Server validates refresh_token
6. Server REVOKES old refresh_token
7. Server issues NEW access_token + NEW refresh_token
8. Client updates stored tokens
9. Client retries request with new access_token
```

**Old token flow still works:**

```
1. Attacker intercepts refresh_token in transit
2. Attacker calls /auth/refresh-token with stolen token
3. Server accepts (token not yet revoked by legitimate user)
4. Server issues tokens to attacker
5. Server stores refresh_token
6. Legitimate user calls /auth/refresh-token with their token
7. Server detects mismatch - token already used!
8. Server REVOKES all tokens for user
9. User receives error, must re-login
```

This double-refresh detection prevents token compromise.

## Password Security

Passwords are hashed using **PBKDF2** (ASP.NET Core Identity default):

```csharp
var passwordHasher = new PasswordHasher<ApplicationUser>();
var hashedPassword = passwordHasher.HashPassword(user, password);
```

**PBKDF2 Properties:**

- Algorithm: PBKDF2-SHA256
- Iterations: 10,000 (default)
- Salt: Randomly generated per user
- Output: Salted hash stored in database

**Password Requirements (configurable):**

```csharp
services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
});
```

| Requirement       | Rule                   |
| ----------------- | ---------------------- |
| Minimum length    | 8 characters           |
| Uppercase letter  | At least 1 (A-Z)       |
| Lowercase letter  | At least 1 (a-z)       |
| Digit             | At least 1 (0-9)       |
| Special character | At least 1 (!@#$%^&\*) |

## Authorization

### Role-Based Access Control (RBAC)

Four predefined roles with hierarchical permissions:

| Role           | Permissions                          | Typical User           |
| -------------- | ------------------------------------ | ---------------------- |
| **SuperAdmin** | All operations, all merchants        | Platform admins        |
| **Merchant**   | Manage own store, add staff, reports | Restaurant owners      |
| **Staff**      | View orders, update statuses         | Waiters, kitchen staff |
| **Guest**      | Read-only public endpoints           | Anonymous customers    |

### Applying Authorization

```csharp
[Authorize]                            // Any authenticated user
[Authorize(Roles = "Merchant")]        // Merchant role only
[Authorize(Roles = "Merchant,SuperAdmin")] // Multiple roles
[AllowAnonymous]                       // No authentication required
```

### Resource-Level Authorization

Roles are necessary but not sufficient. Handlers verify resource ownership:

```csharp
public async Task<UpdateCategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
{
    var currentMerchantId = _currentUserService.MerchantId!.Value;
    var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

    if (category == null)
        throw new NotFoundException("Category", request.Id);

    // Ownership check
    if (category.MerchantId != currentMerchantId)
        throw new ForbiddenException("You do not have permission to modify this category");

    // Proceed with update
    category.Name = request.Name;
    category.Description = request.Description;

    return await _categoryRepository.UpdateAsync(category, cancellationToken);
}
```

**Triple-layer defense:**

1. Authentication check (token valid)
2. Role-based authorization (user has role)
3. Resource-level check (user owns resource)

## Failed Login Handling

### Rate Limiting

Prevents brute force attacks on login endpoint:

```csharp
[EnableRateLimiting(RateLimitPolicies.Login)]
public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
```

**Limits:**

- 5 login attempts per 15 minutes per IP address
- After limit exceeded: HTTP 429 (Too Many Requests)

### Failed Attempt Tracking

```csharp
if (!isPasswordValid)
{
    user.AccessFailedCount++;
    if (user.AccessFailedCount >= 5)
    {
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
    }
    await _userManager.UpdateAsync(user);

    throw new UnauthorizedException("Invalid email or password");
}
```

**Behavior:**

- Failed attempt increments counter
- 5 failed attempts → Account locked 15 minutes
- Duration: LockoutEnd > current time
- Login attempted during lockout → Error

## Session Management

### Token Expiration

Access tokens are **not** revoked server-side:

```csharp
// ✅ Valid: Server stateless, relies on exp claim
var token = new JwtSecurityToken(
    expires: DateTime.UtcNow.AddMinutes(10), // Expiry in claim
    signingCredentials: credentials
);

// Validation
if (token.ValidTo < DateTime.UtcNow)
    throw new SecurityTokenExpiredException("Token expired");
```

### Logout

No explicit logout functionality. Token lifetime ensures timeout:

1. Client deletes access token from memory
2. Client deletes refresh token
3. After 7 days, refresh token expires in database
4. User must re-login

### Forced Session Termination

For emergency access revocation:

```csharp
// Admin action: Revoke all user's refresh tokens
var refreshTokens = await _db.RefreshTokens
    .Where(t => t.UserId == userId && !t.IsRevoked)
    .ToListAsync();

foreach (var token in refreshTokens)
    token.IsRevoked = true;

await _db.SaveChangesAsync();

// User cannot refresh after this point
// Existing access tokens still valid until natural expiry
// User must re-login after expiry
```

## Related Documentation

- [API Overview](overview.md) — Response formats, API structure
- [Authorization Details](../security/authorization.md) — Permission system
- [Data Protection](../security/data-protection.md) — Encryption, TLS
