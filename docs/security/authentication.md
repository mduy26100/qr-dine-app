# Authentication & Passwords

Security implementation for user authentication, password storage, and token management.

## JWT Bearer Tokens

QRDine uses stateless JWT (JSON Web Token) authentication:

```json
{
  "sub": "user-id-uuid",
  "email": "merchant@example.com",
  "merchant_id": "merchant-uuid",
  "role": "Merchant",
  "iat": 1708770896,
  "exp": 1708771496,
  "iss": "https://qrdine.com",
  "aud": "https://qrdine.com"
}
```

**Token Lifecycle:**

1. **Login** — Validate credentials → Issue access + refresh tokens
2. **Access Token** — 10 minutes lifetime; includes merchant_id claim
3. **Refresh Token** — 7 days lifetime; tracked in database
4. **Expiry** — Client receives 401 → Uses refresh token → Gets new access token
5. **Rotation** — Each refresh issues new refresh token; old token invalidated

## Password Security

Passwords are hashed using **PBKDF2** (ASP.NET Core Identity default):

```csharp
var passwordHasher = new PasswordHasher<ApplicationUser>();
var hashedPassword = passwordHasher.HashPassword(user, password);
```

**PBKDF2 Properties:**

- **Algorithm:** PBKDF2-SHA256
- **Iterations:** 10,000 (default, increased from 5000 in .NET 8)
- **Salt:** Randomly generated per user (128-bit)
- **Output:** Salted hash stored securely in database

**Password Verification:**

```csharp
var result = passwordHasher.VerifyHashedPassword(user, hashFromDb, providedPassword);

// result values:
// PasswordVerificationResult.Success: Password matches
// PasswordVerificationResult.SuccessRehashNeeded: Password matches, rehash to newer algorithm
// PasswordVerificationResult.Failed: Password does not match
```

### Password Requirements

Configurable policy enforced on registration and password change:

```csharp
services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;                   // Requires 0-9
    options.Password.RequireLowercase = true;               // Requires a-z
    options.Password.RequireUppercase = true;               // Requires A-Z
    options.Password.RequireNonAlphanumeric = true;         // Requires !@#$%^&*
    options.Password.RequiredUniqueChars = 0;
});
```

| Requirement       | Rule                              |
| ----------------- | --------------------------------- | -------- |
| Minimum length    | 8 characters                      |
| Uppercase letter  | At least 1 (A-Z)                  |
| Lowercase letter  | At least 1 (a-z)                  |
| Digit             | At least 1 (0-9)                  |
| Special character | At least 1 (`!@#$%^&\*()\_+-=[]{} | ;:,.<>`) |

**Example valid passwords:**

- ✅ `SecurePass123!`
- ✅ `Merchant@2026Pro`
- ✅ `MyRestaurant!99`

**Example invalid passwords:**

- ❌ `12345678` (no letters)
- ❌ `password` (no uppercase, digit)
- ❌ `Pass123` (no special char)
- ❌ `Pass!` (too short)

## Refresh Token Rotation

Prevents token replay attacks by rotating on each refresh:

```csharp
public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
{
    // 1. Validate token exists and is valid
    var tokenEntity = await _db.RefreshTokens.FirstOrDefaultAsync(
        x => x.Token == refreshToken
        && !x.IsRevoked
        && x.ExpiryDate > DateTime.UtcNow);

    if (tokenEntity == null)
        throw new ForbiddenException("Refresh token invalid or expired");

    var user = await _userManager.FindByIdAsync(tokenEntity.UserId.ToString());

    // 2. Generate new tokens
    var newAccessToken = GenerateAccessToken(user);
    var newRefreshToken = GenerateRefreshToken();

    // 3. REVOKE old refresh token (critical!)
    tokenEntity.IsRevoked = true;
    await _db.SaveChangesAsync();

    // 4. Store new refresh token
    _db.RefreshTokens.Add(new RefreshToken
    {
        UserId = user.Id,
        Token = newRefreshToken,
        ExpiryDate = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiryDays)
    });
    await _db.SaveChangesAsync();

    return new LoginResponseDto
    {
        AccessToken = newAccessToken,
        RefreshToken = newRefreshToken,
        ExpiresIn = _settings.AccessTokenExpiryMinutes * 60
    };
}
```

### Token Replay Attack Prevention

```
Attack Timeline:
├─ T1: User refreshes token (server issues AT1 + RT1)
├─ T2: Attacker intercepts RT1 in transit
├─ T3: Attacker calls /refresh with stolen RT1
│  └─ Server issues AT2 + RT2 to attacker
├─ T4: Legitimate user calls /refresh with RT1
│  └─ Server detects: RT1 already revoked!
└─ T5: Server revokes ALL tokens for user
   └─ All attacker's tokens invalidated
   └─ User must re-login
```

**Detection mechanism:**

```csharp
if (tokenEntity.IsRevoked)
{
    // Token already used! Possible compromise
    // Revoke all user's tokens as precaution
    var allTokens = await _db.RefreshTokens
        .Where(t => t.UserId == user.Id && !t.IsRevoked)
        .ToListAsync();

    foreach (var token in allTokens)
        token.IsRevoked = true;

    await _db.SaveChangesAsync();

    _logger.LogWarning($"Possible token compromise. Revoked all tokens for user {user.Id}");
    throw new SecurityException("Suspicious activity detected. Please re-login.");
}
```

## Failed Login Handling

### Account Lockout

Protects against brute force attacks:

```csharp
var result = await _userManager.CheckPasswordAsync(user, password);

if (!result)
{
    // Increment failed attempt counter
    user.AccessFailedCount++;

    // Lock after 5 failures
    if (user.AccessFailedCount >= _userManager.MaxFailedAccessAttempts)
    {
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
        _logger.LogWarning($"Account locked for {user.Email} due to failed login attempts");
    }

    await _userManager.UpdateAsync(user);
    throw new UnauthorizedException("Invalid email or password");
}

// Reset counter on successful login
user.AccessFailedCount = 0;
user.LockoutEnd = null;
await _userManager.UpdateAsync(user);
```

**Configuration:**

```csharp
services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});
```

| Setting                   | Value      | Purpose               |
| ------------------------- | ---------- | --------------------- |
| `DefaultLockoutTimeSpan`  | 15 minutes | Lockout duration      |
| `MaxFailedAccessAttempts` | 5 attempts | Lock after N failures |
| `AllowedForNewUsers`      | true       | Apply to new users    |

### Rate Limiting

Additional protection via HTTP-level rate limiting:

```csharp
[EnableRateLimiting(RateLimitPolicies.Login)]
public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
{
    // 5 attempts per 15 minutes per IP address
}
```

Enforced at middleware level (per IP) in addition to per-user account lockout.

## Failed Login Response

Always return generic error message (no user enumeration):

```csharp
// ✅ Good
throw new UnauthorizedException("Invalid email or password");

// ❌ Bad (allows attacker to enumerate users)
if (user == null)
    throw new UnauthorizedException("Email not registered");
if (!result)
    throw new UnauthorizedException("Password incorrect");
```

## Session Management

### Token Expiration

Access tokens are **stateless** (no server revocation):

```csharp
var token = new JwtSecurityToken(
    expires: DateTime.UtcNow.AddMinutes(10),
    signingCredentials: credentials
);

// Validation (token validation only checks expiry, no DB query)
var validationParameters = new TokenValidationParameters
{
    ValidateLifetime = true,  // Check exp claim
    ClockSkew = TimeSpan.FromSeconds(0)  // No grace period
};
```

**Benefits:**

- Stateless (no DB lookup per request)
- Distributed cache-friendly
- Horizontal scaling support

**Tradeoff:**

- Cannot revoke unexpired access tokens
- Refresh tokens provide mitigation

### Logout

No explicit logout endpoint. Token expiry provides timeout:

1. Client deletes access token from memory
2. Client deletes refresh token
3. After 7 days, refresh token expires in database
4. User must re-login

### Forced Session Termination

For immediate access revocation (e.g., compromised account):

```csharp
// Admin action: Revoke all user's refresh tokens
public async Task RevokeAllUserTokensAsync(Guid userId)
{
    var refreshTokens = await _db.RefreshTokens
        .Where(t => t.UserId == userId && !t.IsRevoked)
        .ToListAsync();

    foreach (var token in refreshTokens)
        token.IsRevoked = true;

    await _db.SaveChangesAsync();

    _logger.LogWarning($"All refresh tokens revoked for user {userId}");
}
```

**Effect:**

- Existing access tokens still valid (until natural expiry)
- User cannot refresh after current token expires
- After 10 minutes: User forced to re-login

## Token Generation

### Access Token

```csharp
private string GenerateAccessToken(ApplicationUser user, ApplicationRole role)
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(AppClaimTypes.MerchantId, user.MerchantId.ToString()),
        new Claim(ClaimTypes.Role, role.Name),
        new Claim("iss", _settings.ValidIssuer),
        new Claim("aud", _settings.ValidAudience)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _settings.ValidIssuer,
        audience: _settings.ValidAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
        signingCredentials: credentials
    );

    var tokenHandler = new JwtSecurityTokenHandler();
    return tokenHandler.WriteToken(token);
}
```

### Refresh Token

```csharp
private string GenerateRefreshToken()
{
    // Generate cryptographically secure random token
    var randomNumber = new byte[64];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(randomNumber);
    }

    return Convert.ToBase64String(randomNumber);
}
```

Uses `RandomNumberGenerator` (cryptographically secure) instead of `Random` (predictable).

## Related Documentation

- [API Authentication](../api/authentication.md) — Login flow, token usage
- [Authorization & Permissions](authorization.md) — Roles, permissions
- [Data Protection](data-protection.md) — Encryption, TLS
- [Security Overview](overview.md) — Security architecture
