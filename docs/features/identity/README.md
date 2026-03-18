# Identity Module

User authentication, registration, and role management.

## Quick Overview

The Identity module handles user authentication, registration, JWT token management, and role-based access control using ASP.NET Core Identity.

## Key Features

- ✅ Merchant and staff registration
- ✅ JWT Bearer authentication with refresh tokens
- ✅ Role-based access control (4 roles)
- ✅ Email and phone validation
- ✅ Refresh token rotation (one-time use)
- ✅ User profile management

## System Roles

| Role           | Purpose                    | Who Can Register  |
| -------------- | -------------------------- | ----------------- |
| **SuperAdmin** | Platform administrator     | Seeded on startup |
| **Merchant**   | Restaurant owner           | SuperAdmin only   |
| **Staff**      | Restaurant employee        | Merchant owner    |
| **Guest**      | Public customer (reserved) | Not used          |

## Key Features

- User login with email or phone number
- Merchant registration with merchant profile creation
- Staff registration under merchant
- JWT token generation and validation
- Refresh token management with rotation
- Role and permission system

## Authentication Flow

```
1. User provides email + password
   ↓
2. System validates credentials
   ↓
3. System generates JWT token (10-min TTL)
   ↓
4. System generates refresh token (7-day TTL)
   ↓
5. Return tokens to client
   ↓
6. Client uses JWT in Authorization header
   ↓
7. When JWT expires, use refresh token to get new JWT
```

## API Endpoints

| Method | Path                              | Auth          | Purpose                                  |
| ------ | --------------------------------- | ------------- | ---------------------------------------- |
| `POST` | `/api/v1/auth/login`              | Public        | Login with email/phone + password        |
| `POST` | `/api/v1/auth/refresh-token`      | Public        | Get new access token from refresh token  |
| `POST` | `/api/v1/users/register-merchant` | SuperAdmin    | Register merchant + create owner account |
| `POST` | `/api/v1/users/register-staff`    | Merchant      | Register staff under current merchant    |
| `GET`  | `/api/v1/users/profile`           | Authenticated | Get current user profile                 |

## Documentation

- **[Complete Identity Module Documentation](identity-module.md)** — Full documentation with all models, commands, services, and flow details

---

**Reference:** See also [Features Overview](../) for other modules and [Security Overview](../../security/) for authentication details.
