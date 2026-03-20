# PayOS Payment Integration - Complete Technical Reference

## Executive Summary

PayOS is a Vietnamese payment gateway integrated into QRDine for subscription plan purchases. The system converts orders to PayOS checkout links, verifies payments via webhooks, and automatically assigns subscriptions upon successful payment.

### Key Features

- ✓ Secure payment link generation
- ✓ Webhook signature verification
- ✓ Automatic subscription activation
- ✓ Idempotent webhook handling
- ✓ Multi-currency support (VND)
- ✓ Transaction history tracking
- ✓ Insufficient payment detection

### Current Status

- **Implementation**: Complete (100%)
- **Documentation**: New (created with this guide)
- **Testing**: Unit tests exist, integration tests recommended
- **Production Ready**: Yes
- **Configuration Required**: Yes (see below)

---

## Quick Start for Developers

### 1. Configuration (First Time Setup)

```json
// appsettings.Development.json
{
  "PayOS": {
    "ClientId": "get-from-payos-dashboard",
    "ApiKey": "get-from-payos-dashboard",
    "ChecksumKey": "get-from-payos-dashboard"
  },
  "FrontendSettings": {
    "BaseUrl": "http://localhost:5173"
  }
}
```

### 2. Test Payment Flow

```bash
# Create checkout link
curl -X POST http://localhost:7288/api/v1/management/billing/plans/checkout \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"planId":"YOUR_PLAN_GUID"}'

# Receive in response:
# https://payos.vn/web/...checkout-url...

# Complete payment on PayOS (test mode)

# Webhook delivered automatically
# Check logs for webhook processing confirmation
```

### 3. Common Tasks

**Check subscription status**:

```bash
curl -X GET http://localhost:7288/api/v1/management/billing/subscription \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**View payment history** (database query):

```sql
SELECT * FROM Transactions
WHERE MerchantId = 'merchant-guid'
ORDER BY PaidAt DESC;
```

---

## Documentation Structure

### 📘 Main Documentation Files

| File                                                             | Purpose                                                                                      | Audience                                    |
| ---------------------------------------------------------------- | -------------------------------------------------------------------------------------------- | ------------------------------------------- |
| [payos-integration.md](payos-integration.md)                     | **Complete reference guide** - Configuration, architecture, API details, error handling, FAQ | Backend developers, DevOps, QA              |
| [payos-architecture-diagrams.md](payos-architecture-diagrams.md) | **Visual architecture** - Flow diagrams, state machines, data models, error flows            | Architects, team leads, developers          |
| [payos-api-reference.md](payos-api-reference.md)                 | **API & code examples** - Request/response samples, frontend integration, testing            | Frontend developers, API consumers, testers |
| [README.md](README.md)                                           | **External services overview** - Links to all external integrations                          | Project overview                            |

### 🔍 Quick Reference Section

**By Role**:

- **Backend Developer**: Start with [payos-integration.md](payos-integration.md) → Configuration section
- **Frontend Developer**: Start with [payos-api-reference.md](payos-api-reference.md) → Frontend Integration
- **DevOps/Infrastructure**: [payos-integration.md](payos-integration.md) → Configuration & Environment Variables
- **QA/Tester**: [payos-api-reference.md](payos-api-reference.md) → Testing section
- **Architect**: [payos-architecture-diagrams.md](payos-architecture-diagrams.md)

---

## Complete Payment Flow at a Glance

```
1. Merchant clicks "Upgrade Plan"
   └─ POST /api/v1/management/billing/plans/checkout
      └─ Response: https://payos.vn/web/...checkout-url...

2. Redirect → PayOS payment page
   └─ Merchant completes payment
      └─ PayOS processes payment

3. PayOS sends webhook
   └─ POST /api/v1/webhooks/payos
      ├─ Signature verified
      ├─ Amount validated
      └─ Status updated

4. Automatic subscription assignment
   ├─ If new: Create subscription
   ├─ If renewal: Extend dates
   ├─ If upgrade: Change plan
   └─ Record transaction

5. Subscription active
   └─ Frontend shows success message
      └─ Customer can use service
```

---

## Key File Locations

### Implementation Files

```
Source Code Structure
├── src/QRDine.API/
│   ├── Controllers/Webhooks/
│   │   └── PayOSWebhookController.cs          # Webhook endpoint
│   └── DependencyInjection/Infrastructure/
│       └── PayOsServiceCollectionExtensions.cs # DI setup
│
├── src/QRDine.Application/
│   └── Features/Billing/Plans/Commands/
│       ├── CreateCheckoutLink/
│       │   ├── CreateCheckoutLinkCommand.cs
│       │   └── CreateCheckoutLinkCommandHandler.cs
│       ├── ProcessPaymentWebhook/
│       │   ├── ProcessPaymentWebhookCommand.cs
│       │   └── ProcessPaymentWebhookCommandHandler.cs
│       └── AssignPlan/
│           ├── AssignPlanCommand.cs
│           └── AssignPlanCommandHandler.cs
│
├── src/QRDine.Application.Common/
│   └── Abstractions/PayOS/
│       ├── IPayOSService.cs                   # Service interface
│       └── Models/
│           └── PaymentLinkRequestDto.cs       # Request model
│
├── src/QRDine.Infrastructure/
│   ├── Configuration/
│   │   ├── PayOsSettings.cs                   # Configuration class
│   │   └── FrontendSettings.cs
│   └── PayOS/
│       └── PayOSService.cs                    # Service implementation
│
└── src/QRDine.Domain/Billing/
    ├── Plan.cs
    ├── Subscription.cs
    ├── SubscriptionCheckout.cs
    └── Transaction.cs
```

### Configuration Files

```
Configuration Hierarchy
├── appsettings.json                           # Base config
├── appsettings.Development.json               # Dev overrides
├── appsettings.Production.json                # Production
│   (uses environment variables)
├── .env.example                               # Environment template
└── docker/.env                                # Docker environment
```

### Test Files

```
Test Coverage
└── tests/QRDine.Application.Tests/
    ├── Features/Billing/Plans/Commands/
    │   ├── CreateCheckoutLink/
    │   │   └── CreateCheckoutLinkCommandHandlerTests.cs
    │   ├── ProcessPaymentWebhook/
    │   │   └── ProcessPaymentWebhookCommandHandlerTests.cs
    │   └── AssignPlan/
    │       └── AssignPlanCommandHandlerTests.cs
    └── Common/Mocks/Billing/
        └── BillingExternalServicesMocks.cs
```

---

## Configuration Checklist

### ✓ Development Environment

- [ ] Get PayOS credentials from [payos.vn](https://payos.vn)
- [ ] Add PayOS section to `appsettings.Development.json`
- [ ] Set `FrontendSettings.BaseUrl` to your local frontend URL
- [ ] Run application and verify no startup errors
- [ ] Test checkout endpoint with Postman/cURL

### ✓ Production Environment

- [ ] Create PayOS merchant account (production)
- [ ] Set environment variables:
  ```
  PAYOS_CLIENT_ID=xxx
  PAYOS_API_KEY=yyy
  PAYOS_CHECKSUM_KEY=zzz
  FRONTEND_BASE_URL=https://qrdine.me
  ```
- [ ] Configure webhook URL in PayOS dashboard: `https://api.qrdine.me/api/v1/webhooks/payos`
- [ ] Update `appsettings.Production.json`
- [ ] Enable HTTPS
- [ ] Test with live payment (small amount)
- [ ] Monitor logs for first webhook

### ✓ Deployment Checklist

- [ ] Database migrations applied (Subscriptions, Transactions, Plans)
- [ ] Redis cache configured for `MerchantActiveStatus`
- [ ] CORS settings include PayOS return URLs
- [ ] Webhook endpoint exposed publicly (`/api/v1/webhooks/payos`)
- [ ] SSL/TLS certificate valid
- [ ] Logging configured for `PayOS*` errors
- [ ] Alerts set for webhook failures
- [ ] Backup plan for manual subscription assignment

---

## Data Model Summary

### Entities

| Entity                 | Purpose                       | Key Fields                                |
| ---------------------- | ----------------------------- | ----------------------------------------- |
| `Plan`                 | Subscription plan definitions | Code, Name, Price, DurationDays           |
| `SubscriptionCheckout` | Payment order tracking        | OrderCode, Amount, Status, FailureReason  |
| `Subscription`         | Active merchant subscriptions | MerchantId, PlanId, StartDate, EndDate    |
| `Transaction`          | Payment history record        | Amount, Method, Status, PaidAt, Reference |

### Enums

| Enum                 | Values                                          | Usage                         |
| -------------------- | ----------------------------------------------- | ----------------------------- |
| `PaymentStatus`      | Pending, Success, Failed, Refunded              | Checkout & Transaction status |
| `PaymentMethod`      | PayOS, BankTransfer, Manual_Admin, System_Grant | How payment was made          |
| `SubscriptionStatus` | Trialing, Active, Expired, Cancelled            | Subscription state            |

---

## Architecture Patterns

### 1. CQRS (Command Query Responsibility Segregation)

**Commands** (state-changing):

- `CreateCheckoutLinkCommand` → Creates checkout record + payment link
- `ProcessPaymentWebhookCommand` → Processes webhook + assigns subscription
- `AssignPlanCommand` → Assigns plan to merchant (manual admin)

**Queries** (read-only):

- `GetLatestSubscriptionInfoSpec` → Fetch current subscription
- `CheckActiveSubscriptionSpec` → Check if subscription is active

### 2. Repository Pattern

```csharp
interface ISubscriptionCheckoutRepository : IRepository<SubscriptionCheckout>
interface ISubscriptionRepository : IRepository<Subscription>
interface ITransactionRepository : IRepository<Transaction>
```

### 3. Service Layer

**PayOSService**: Abstraction over PayOS SDK

```csharp
Task<string> CreatePaymentLinkAsync(PaymentLinkRequestDto request);
```

**SubscriptionService**: Business logic for subscriptions

```csharp
Task<Subscription> AssignPlanAsync(Guid merchantId, Guid planId, ...);
Task<bool> IsSubscriptionActiveAsync(Guid merchantId);
```

### 4. Specification Pattern

Encapsulates database queries:

```csharp
new SubscriptionCheckoutByOrderCodeSpec(orderCode)
new GetSubscriptionByMerchantIdSpec(merchantId)
new CheckActiveSubscriptionSpec(merchantId)
```

---

## API Endpoints Overview

### For Merchants (Protected by JWT)

| Endpoint                                    | Method | Purpose                  |
| ------------------------------------------- | ------ | ------------------------ |
| `/api/v1/management/billing/plans/checkout` | POST   | Create checkout link     |
| `/api/v1/management/billing/subscription`   | GET    | Get current subscription |
| `/api/v1/management/billing/plans`          | GET    | List available plans     |

### For System (Unprotected)

| Endpoint                 | Method | Purpose                  |
| ------------------------ | ------ | ------------------------ |
| `/api/v1/webhooks/payos` | POST   | Receive payment webhooks |

---

## Error Handling Strategy

### Levels

1. **API Level**: Validation errors → 400 Bad Request
2. **Service Level**: Business rule violations → 409 Conflict / 422 Unprocessable
3. **Infrastructure Level**: External API failures → 503 Service Unavailable
4. **Webhook Level**: Signature failures → Log + Retry by PayOS

### Key Error Scenarios

| Scenario                  | Handling                                    | User Impact                 |
| ------------------------- | ------------------------------------------- | --------------------------- |
| Insufficient payment      | Mark as Failed, retry allowed               | User retries payment        |
| Webhook signature invalid | Log alert, skip processing                  | Payment status unknown      |
| PayOS API timeout         | Retry with exponential backoff              | User retries checkout       |
| Duplicate webhook         | Idempotent check, no duplicate subscription | No issue                    |
| Plan not found            | Error + rollback                            | Checkout fails with message |

---

## Performance Considerations

### Current Optimizations

- **Caching**: `MerchantActiveStatus` cached with TTL
- **Async/Await**: All I/O operations non-blocking
- **HttpClient**: Configured with 30-second timeout
- **Database**: Indexed queries on `MerchantId`, `OrderCode`
- **Transactions**: Database transaction for atomicity in webhook handler

### Monitoring Metrics

- Checkout link creation time
- Webhook processing time
- Payment success/failure rates
- Cache hit rates
- PayOS API response times

---

## Security Considerations

### 1. Webhook Signature Verification ✓

- All webhooks verified using HMAC-SHA256
- Signature computed with ChecksumKey
- Invalid signatures rejected immediately

### 2. PCI Compliance ✓

- No card data stored in QRDine
- All payment processing delegated to PayOS
- Only transaction references stored

### 3. Authentication ✓

- JWT tokens required for merchant endpoints
- Anonymous access allowed only for webhook endpoint
- CORS configured for trusted origins

### 4. HTTPS ✓

- All external communications encrypted
- PayOS endpoints HTTPS only
- Webhook URL requires HTTPS

### 5. Secrets Management ✓

- Credentials stored in environment variables
- Never committed to version control
- Separate Production/Development configurations

---

## Common Questions

**Q: How long does payment processing take?**
A: Typically 1-5 seconds after user completes payment on PayOS. Webhook delivery guaranteed within minutes.

**Q: What if webhook fails?**
A: PayOS automatically retries (6 times over ~3 hours). Handler is idempotent - safe to receive same webhook multiple times.

**Q: Can merchant change plans during trial?**
A: Yes. They can upgrade/downgrade anytime. Payment immediately activates new plan.

**Q: Is there a refund system?**
A: Not currently implemented. Consider as future enhancement.

**Q: How do we handle failed payments?**
A: System marks checkout as Failed with reason. Merchant can retry with different payment method.

**Q: What's the merchant UI flow?**
A: Select plan → Click "Upgrade" → Redirected to PayOS → Complete payment → Redirected back to success page → Subscription active.

---

## Troubleshooting Quick Reference

| Issue                                  | Check                                  | Solution                                             |
| -------------------------------------- | -------------------------------------- | ---------------------------------------------------- |
| Config error on startup                | `appsettings.json` has `PayOS` section | Add PayOS configuration                              |
| Checkout link 404                      | JWT token valid?                       | Include `Authorization: Bearer {token}` header       |
| Webhook not received                   | PayOS dashboard webhook URL            | Set to `https://api.qrdine.me/api/v1/webhooks/payos` |
| Payment verification fails             | ChecksumKey matches PayOS dashboard    | Regenerate from PayOS dashboard                      |
| Subscription not created after payment | Check logs for webhook processing      | Look for error in application logs                   |
| 500 error on webhook                   | Exception in handler                   | Check application error logs                         |

---

## Next Steps

### For Implementation

1. ✓ Review architecture diagrams
2. ✓ Setup PayOS account
3. ✓ Configure development environment
4. ✓ Run existing tests
5. ✓ Test E2E payment flow
6. ✓ Deploy to production

### For Enhancement

1. Add refund functionality
2. Implement subscription pause/resume
3. Add payment history dashboard
4. Implement auto-renewal notifications
5. Add payment method alternatives (credit cards)
6. Implement tiered pricing
7. Add proration logic for mid-cycle upgrades

### For Monitoring

1. Setup Datadog/New Relic APM
2. Configure alerts for webhook failures
3. Monitor payment success rates
4. Track API response times
5. Setup dashboard for payment analytics

---

## Support & Resources

### Internal Documentation

- [Billing Feature Module](../features/billing/)
- [Database Schema](../database/schema.md)
- [API Conventions](../api/conventions.md)
- [Error Handling Patterns](../architecture/patterns-and-design.md#error-handling)

### External Resources

- [PayOS Official Documentation](https://payos.vn/docs/)
- [PayOS Webhook Guide](https://payos.vn/docs/webhooks)
- [PayOS Test Credentials](https://payos.vn/docs/testing)

### Team Contacts

- Lead Developer: [Backend Team]
- DevOps: [Infrastructure Team]
- QA: [Testing Team]

---

## Document Version

| Version | Date       | Changes                                                |
| ------- | ---------- | ------------------------------------------------------ |
| 1.0     | 2025-03-21 | Initial documentation                                  |
|         |            | - payos-integration.md (complete reference)            |
|         |            | - payos-architecture-diagrams.md (visual architecture) |
|         |            | - payos-api-reference.md (code examples & testing)     |
|         |            | - payos-complete-reference.md (this file)              |

---

## Appendix: Command Quick Reference

```bash
# Create checkout link
curl -X POST http://localhost:7288/api/v1/management/billing/plans/checkout \
  -H "Authorization: Bearer JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"planId":"PLAN_GUID"}'

# Get subscription info
curl -X GET http://localhost:7288/api/v1/management/billing/subscription \
  -H "Authorization: Bearer JWT_TOKEN"

# Check database status
sqlcmd -S localhost -U sa -P "Password123" -Q \
  "SELECT * FROM Subscriptions WHERE MerchantId = 'MERCHANT_GUID'"

# View latest logs
docker logs qrdine-api | grep "PayOS" | tail -100

# Run unit tests
dotnet test tests/QRDine.Application.Tests/\
  --filter "Category=PayOS"
```

---

**Last Updated**: March 21, 2025
**Status**: Complete & Production Ready
**Maintained By**: QRDine Development Team
