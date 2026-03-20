# PayOS Payment Integration

## Overview

PayOS is a Vietnamese payment gateway integrated into QRDine for subscription plan payments. It enables merchants to accept online payments via QR codes, bank transfers, and digital wallets.

**Integration Type**: Micropayment aggregator
**Payment Methods**: QR Code, Bank Transfer, Digital Wallets
**Primary Use**: Subscription plan transactions

---

## Table of Contents

1. [Configuration](#configuration)
2. [Service Architecture](#service-architecture)
3. [Payment Flow](#payment-flow)
4. [Domain Models](#domain-models)
5. [API Implementation](#api-implementation)
6. [Webhook Handling](#webhook-handling)
7. [Error Handling](#error-handling)
8. [Testing](#testing)
9. [Troubleshooting](#troubleshooting)

---

## Configuration

### 1. appsettings.json

```json
"PayOS": {
  "ClientId": "your_client_id",
  "ApiKey": "your_api_key",
  "ChecksumKey": "your_checksum_key"
}
```

### 2. Environment Variables

| Variable             | Description                             | Example                                |
| -------------------- | --------------------------------------- | -------------------------------------- |
| `PAYOS_CLIENT_ID`    | PayOS merchant client ID                | `16d47110-8c6b-4ff8-90d2-e2fa81f47831` |
| `PAYOS_API_KEY`      | PayOS API authentication key            | `bb6de904-5d37-4068-a15b-305ecae53e38` |
| `PAYOS_CHECKSUM_KEY` | PayOS checksum for webhook verification | `2be7e0fdea3baf75b1f61e4e717762a2... ` |

### 3. Frontend Configuration

The frontend base URL is required for payment return/cancel URLs:

```json
"FrontendSettings": {
  "BaseUrl": "http://localhost:5173"  // or production URL
}
```

**Return URLs used**:

- Success: `{frontendBaseUrl}/management/billing/success`
- Cancel: `{frontendBaseUrl}/management/billing/cancel`

### 4. PayOS Account Setup

1. Sign up at [https://payos.vn/](https://payos.vn/)
2. Create API credentials in dashboard
3. Configure webhook URL: `{apiBaseUrl}/api/v1/webhooks/payos`
4. Copy ClientId, ApiKey, and ChecksumKey to configuration

---

## Service Architecture

### 1. PayOsSettings Configuration Class

**Location**: `src/QRDine.Infrastructure/Configuration/PayOsSettings.cs`

```csharp
namespace QRDine.Infrastructure.Configuration
{
    public class PayOsSettings
    {
        public string ClientId { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public string ChecksumKey { get; set; } = default!;
    }
}
```

### 2. IPayOSService Interface

**Location**: `src/QRDine.Application.Common/Abstractions/PayOS/IPayOSService.cs`

```csharp
namespace QRDine.Application.Common.Abstractions.PayOS
{
    public interface IPayOSService
    {
        /// <summary>
        /// Creates a PayOS payment link for order checkout.
        /// </summary>
        /// <param name="request">Payment link request containing order details</param>
        /// <returns>PayOS checkout URL for user redirection</returns>
        Task<string> CreatePaymentLinkAsync(PaymentLinkRequestDto request);
    }
}
```

### 3. PayOSService Implementation

**Location**: `src/QRDine.Infrastructure/PayOS/PayOSService.cs`

```csharp
namespace QRDine.Infrastructure.PayOS
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOSClient _payOSClient;

        public PayOSService(PayOSClient payOSClient)
        {
            _payOSClient = payOSClient;
        }

        public async Task<string> CreatePaymentLinkAsync(PaymentLinkRequestDto request)
        {
            var paymentData = new CreatePaymentLinkRequest
            {
                OrderCode = request.OrderCode,
                Amount = request.Amount,
                Description = request.Description,
                CancelUrl = request.CancelUrl,
                ReturnUrl = request.ReturnUrl
            };

            var paymentLink = await _payOSClient.PaymentRequests.CreateAsync(paymentData);
            return paymentLink.CheckoutUrl;
        }
    }
}
```

**Key Points**:

- Uses PayOSClient from official PayOS SDK
- Maps QRDine DTO to PayOS SDK request object
- Returns checkout URL for frontend redirection
- Timeout: 30 seconds (HTTP client)

### 4. Dependency Injection Setup

**Location**: `src/QRDine.API/DependencyInjection/Infrastructure/PayOsServiceCollectionExtensions.cs`

```csharp
namespace QRDine.API.DependencyInjection.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPayOS(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Load settings from configuration
            var settings = configuration.GetSection("PayOS").Get<PayOsSettings>()
                ?? throw new Exception("PayOS configuration is missing");

            // Create HTTP client with 30-second timeout
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Initialize PayOS SDK client
            var payOSClient = new PayOSClient(new PayOSOptions
            {
                ClientId = settings.ClientId,
                ApiKey = settings.ApiKey,
                ChecksumKey = settings.ChecksumKey,
                HttpClient = httpClient
            });

            // Register in DI
            services.AddSingleton(payOSClient);
            services.AddScoped<IPayOSService, PayOSService>();

            return services;
        }
    }
}
```

**Registration called in**: `src/QRDine.API/DependencyInjection/ServiceCollectionExtensions.cs` → `AddInfrastructure()` method

---

## Payment Flow

### Complete Payment Workflow

```
┌─────────────────────────────────────────────────────────────────────────┐
│ 1. MERCHANT INITIATES PLAN CHECKOUT                                     │
├─────────────────────────────────────────────────────────────────────────┤
│ Endpoint: POST /api/v1/management/billing/plans/checkout                │
│ Command:  CreateCheckoutLinkCommand                                     │
│           └─ PlanId: Guid                                               │
│                                                                          │
│ Returns: Checkout URL (string)                                          │
└──────────────────────────────────────────┬──────────────────────────────┘
                                           │
┌──────────────────────────────────────────┴──────────────────────────────┐
│ 2. ORDER CODE GENERATION & CHECKOUT RECORD CREATION                     │
├─────────────────────────────────────────────────────────────────────────┤
│ Handler: CreateCheckoutLinkCommandHandler                               │
│                                                                          │
│ Steps:                                                                   │
│ a) Authenticate merchant via ICurrentUserService                        │
│ b) Load Plan entity from database                                       │
│ c) Check if plan is active                                              │
│ d) Determine order prefix (Mua/Gia han/Nang cap)                       │
│    - Mua: New subscription                                              │
│    - Gia han: Same plan renewal                                         │
│    - Nang cap: Plan upgrade                                             │
│                                                                          │
│ Generate OrderCode:                                                     │
│   long.Parse(DateTimeOffset.UtcNow.ToString("yyMMddHHmmssfff"))        │
│   Example: 2602212345678 (timestamp-based unique ID)                   │
│                                                                          │
│ Create SubscriptionCheckout record:                                     │
│   {                                                                      │
│     OrderCode: 2602212345678,                                          │
│     MerchantId: merchant-guid,                                          │
│     PlanId: plan-guid,                                                  │
│     Amount: plan.Price (decimal),                                       │
│     Status: PaymentStatus.Pending,                                      │
│     PlanSnapshotName: plan.Name                                         │
│   }                                                                      │
└──────────────────────────────────────────┬──────────────────────────────┘
                                           │
┌──────────────────────────────────────────┴──────────────────────────────┐
│ 3. PAYMENT LINK CREATION                                                │
├─────────────────────────────────────────────────────────────────────────┤
│ Service: IPayOSService.CreatePaymentLinkAsync()                         │
│                                                                          │
│ Description Construction (max 25 chars):                                │
│   Format: "{prefix} {planCode} {merchantIdFirstSixChars}"              │
│   Example: "Mua PLN_01 AB1234"                                         │
│                                                                          │
│ Request to PayOS SDK:                                                   │
│   {                                                                      │
│     OrderCode: 2602212345678,                                          │
│     Amount: 99000 (VND - integer),                                     │
│     Description: "Mua PLN_01 AB1234",                                  │
│     CancelUrl: "http://localhost:5173/management/billing/cancel",      │
│     ReturnUrl: "http://localhost:5173/management/billing/success"      │
│   }                                                                      │
│                                                                          │
│ Returns: CheckoutUrl (string)                                           │
└──────────────────────────────────────────┬──────────────────────────────┘
                                           │
┌──────────────────────────────────────────┴──────────────────────────────┐
│ 4. FRONTEND REDIRECTION                                                 │
├─────────────────────────────────────────────────────────────────────────┤
│ Frontend redirects user to PayOS checkout page                          │
│ URL: https://payos.vn/web/.../{checkoutUrl}                           │
│                                                                          │
│ User completes payment on PayOS platform                                │
└──────────────────────────────────────────┬──────────────────────────────┘
                                           │
┌──────────────────────────────────────────┴──────────────────────────────┐
│ 5. WEBHOOK RECEPTION & VERIFICATION                                     │
├─────────────────────────────────────────────────────────────────────────┤
│ Endpoint: POST /api/v1/webhooks/payos                                   │
│ Controller: PayOSWebhookController.HandleWebhook()                      │
│                                                                          │
│ Request Body (Webhook object):                                          │
│   {                                                                      │
│     "code": "00",          // Success code                             │
│     "orderCode": 2602212345678,                                       │
│     "amount": 99000,                                                    │
│     "reference": "bank-ref-12345",                                     │
│     "signature": "payos-signature"                                     │
│   }                                                                      │
│                                                                          │
│ Signature Verification:                                                 │
│   _payOSClient.Webhooks.VerifyAsync(webhookBody)                       │
│   Uses ChecksumKey to verify webhook integrity                         │
└──────────────────────────────────────────┬──────────────────────────────┘
                                           │
┌──────────────────────────────────────────┴──────────────────────────────┐
│ 6. PAYMENT WEBHOOK PROCESSING                                           │
├─────────────────────────────────────────────────────────────────────────┤
│ Command: ProcessPaymentWebhookCommand                                   │
│   - OrderCode: long                                                     │
│   - Amount: long (received VND amount)                                 │
│   - Reference: string (bank reference)                                 │
│                                                                          │
│ Handler: ProcessPaymentWebhookCommandHandler                            │
│                                                                          │
│ Steps:                                                                   │
│ a) Find SubscriptionCheckout by OrderCode                              │
│ b) Verify checkout exists and is Pending                               │
│ c) Verify received amount >= required amount                           │
│    - If insufficient: Mark as Failed with reason                       │
│ d) BEGIN TRANSACTION                                                    │
│ e) Call SubscriptionService.AssignPlanAsync()                          │
│    └─ Assign or extend subscription                                    │
│    └─ Create Transaction record                                        │
│    └─ Update cache                                                     │
│ f) Update SubscriptionCheckout Status → Success                        │
│ g) COMMIT TRANSACTION                                                   │
│ h) Return Ok response to PayOS                                         │
└──────────────────────────────────────────┬──────────────────────────────┘
                                           │
┌──────────────────────────────────────────┴──────────────────────────────┐
│ 7. SUBSCRIPTION ASSIGNMENT                                              │
├─────────────────────────────────────────────────────────────────────────┤
│ ISubscriptionService.AssignPlanAsync()                                  │
│                                                                          │
│ Logic:                                                                   │
│ - Load Merchant (validate exists)                                      │
│ - Load Plan (validate exists & active)                                 │
│ - Load existing Subscription (if any)                                  │
│                                                                          │
│ If NO existing subscription:                                            │
│   Create new Subscription {                                            │
│     MerchantId,                                                        │
│     PlanId,                                                            │
│     Status: Active,                                                    │
│     StartDate: now,                                                    │
│     EndDate: now + plan.DurationDays,                                 │
│     AdminNote                                                          │
│   }                                                                      │
│                                                                          │
│ If existing subscription with SAME plan:                               │
│   Extend EndDate by plan.DurationDays                                 │
│                                                                          │
│ If existing subscription with DIFFERENT plan:                          │
│   Replace: Set new PlanId, reset StartDate, new EndDate               │
│   Update Status to Active                                              │
│                                                                          │
│ Create Transaction record:                                             │
│   {                                                                      │
│     MerchantId,                                                        │
│     PlanId,                                                            │
│     SubscriptionId,                                                    │
│     Amount: overrideAmount (from webhook),                            │
│     Method: BankTransfer (PayOS → BankTransfer enum),                │
│     Status: Success,                                                   │
│     PaidAt: now,                                                       │
│     AdminNote: "Thanh toán PayOS. Reference: {reference}"             │
│   }                                                                      │
│                                                                          │
│ Invalidate cache:                                                       │
│   CacheService.RemoveAsync(MerchantActiveStatus)                       │
└──────────────────────────────────────────┬──────────────────────────────┘
                                           │
┌──────────────────────────────────────────┴──────────────────────────────┐
│ 8. COMPLETION & RESPONSE                                                │
├─────────────────────────────────────────────────────────────────────────┤
│ Database state:                                                         │
│  - SubscriptionCheckout: Status = Success                              │
│  - Subscription: Extended or newly created                             │
│  - Transaction: Recorded with payment details                          │
│                                                                          │
│ Webhook response:                                                       │
│  { "success": true }                                                    │
│                                                                          │
│ Frontend behavior:                                                      │
│  User redirected to success page                                       │
│  Subscription becomes active immediately                               │
└─────────────────────────────────────────────────────────────────────────┘
```

### Step-by-Step Breakdown

#### Step 1: Create Checkout Link Request

**Endpoint**: `POST /api/v1/management/billing/plans/checkout`

**Request**:

```csharp
public record CreateCheckoutLinkCommand(Guid PlanId) : IRequest<string>;
```

**Response**:

```
"https://payos.vn/web/...[long-url]"
```

#### Step 2-3: Generate Order Code & Create Checkout Record

**OrderCode Generation**:

```csharp
long orderCode = long.Parse(DateTimeOffset.UtcNow.ToString("yyMMddHHmmssfff"));
// Result: 2602212345678 (YYMMDDHHmmssfff format)
```

**SubscriptionCheckout Record Structure**:

```csharp
new SubscriptionCheckout
{
    Id = Guid (auto),
    OrderCode = 2602212345678,
    MerchantId = Guid.Parse("merchant-uuid"),
    PlanId = Guid.Parse("plan-uuid"),
    Amount = 99000m,
    Status = PaymentStatus.Pending,
    PlanSnapshotName = "Pro Plan",
    CreatedAt = now,
    UpdatedAt = now
}
```

#### Step 4: Create PayOS Payment Link

**PaymentLinkRequestDto**:

```csharp
public class PaymentLinkRequestDto
{
    public long OrderCode { get; set; }         // 2602212345678
    public int Amount { get; set; }             // 99000 (VND)
    public string Description { get; set; }     // "Mua PLN_01 AB1234"
    public string CancelUrl { get; set; }       // Frontend cancel URL
    public string ReturnUrl { get; set; }       // Frontend success URL
}
```

#### Step 5-6: Webhook Reception & Processing

**Webhook Payload Received**:

```json
{
  "code": "00",
  "orderCode": 2602212345678,
  "amount": 99000,
  "reference": "TRANSFER2602212345678",
  "signature": "payos-checksum-signature"
}
```

**Handler Process**:

```csharp
var webhookData = await _payOSClient.Webhooks.VerifyAsync(webhookBody);

if (webhookData.Code == "00")  // Success code
{
    var command = new ProcessPaymentWebhookCommand(
        webhookData.OrderCode,      // 2602212345678
        webhookData.Amount,         // 99000
        webhookData.Reference       // "TRANSFER2602212345678"
    );

    await _mediator.Send(command);
}
```

#### Step 7: Subscription Assignment

**Transaction Record Created**:

```csharp
new Transaction
{
    Id = Guid (auto),
    MerchantId = Guid.Parse("merchant-uuid"),
    SubscriptionId = subscription.Id,
    PlanId = plan.Id,
    Amount = 99000m,
    Method = PaymentMethod.BankTransfer,  // PayOS processed as bank transfer
    Status = PaymentStatus.Success,
    PaidAt = DateTime.UtcNow,
    AdminNote = "Thanh toán PayOS. Reference: TRANSFER2602212345678",
    CreatedAt = now,
    UpdatedAt = now
}
```

---

## Domain Models

### 1. Plan Entity

**Location**: `src/QRDine.Domain/Billing/Plan.cs`

```csharp
public class Plan : BaseEntity
{
    public string Code { get; set; } = default!;              // "PLN_01"
    public string Name { get; set; } = default!;              // "Pro Plan"
    public decimal Price { get; set; }                        // 99000m (VND)
    public int DurationDays { get; set; }                     // 30
    public bool IsActive { get; set; } = true;

    public virtual FeatureLimit FeatureLimit { get; set; } = default!;
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new();
}
```

### 2. SubscriptionCheckout Entity

**Location**: `src/QRDine.Domain/Billing/SubscriptionCheckout.cs`

```csharp
public class SubscriptionCheckout : BaseEntity, IMustHaveMerchant
{
    public long OrderCode { get; set; }                      // 2602212345678
    public Guid MerchantId { get; set; }
    public Guid PlanId { get; set; }
    public decimal Amount { get; set; }                      // 99000m
    public string? FailureReason { get; set; }              // "Customer transferred insufficient amount"
    public PaymentStatus Status { get; set; }               // Pending/Success/Failed/Refunded
    public string? PlanSnapshotName { get; set; }           // Snapshot at time of checkout

    public virtual Merchant Merchant { get; set; } = default!;
    public virtual Plan Plan { get; set; } = default!;
}
```

### 3. Subscription Entity

**Location**: `src/QRDine.Domain/Billing/Subscription.cs`

```csharp
public class Subscription : BaseEntity, IMustHaveMerchant
{
    public Guid MerchantId { get; set; }
    public Guid PlanId { get; set; }
    public SubscriptionStatus Status { get; set; }          // Active/Expired/Cancelled/Trialing
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? AdminNote { get; set; }

    public virtual Merchant Merchant { get; set; } = default!;
    public virtual Plan Plan { get; set; } = default!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new();
}
```

### 4. Transaction Entity

**Location**: `src/QRDine.Domain/Billing/Transaction.cs`

```csharp
public class Transaction : BaseEntity, IMustHaveMerchant
{
    public Guid MerchantId { get; set; }
    public Guid SubscriptionId { get; set; }
    public Guid PlanId { get; set; }
    public decimal Amount { get; set; }                      // Actual paid amount
    public string? ProviderReference { get; set; }          // PayOS reference
    public PaymentStatus Status { get; set; }               // Success/Failed/Pending/Refunded
    public PaymentMethod Method { get; set; }               // BankTransfer/PayOS/Manual_Admin/System_Grant
    public DateTime? PaidAt { get; set; }
    public string? AdminNote { get; set; }
    public string? PlanSnapshotName { get; set; }

    public virtual Plan Plan { get; set; } = default!;
    public virtual Merchant Merchant { get; set; } = default!;
    public virtual Subscription Subscription { get; set; } = default!;
}
```

### 5. Enums

**PaymentStatus** (`src/QRDine.Domain/Enums/PaymentStatus.cs`):

```csharp
public enum PaymentStatus
{
    Pending = 1,    // Waiting for payment
    Success = 2,    // Payment received
    Failed = 3,     // Payment failed
    Refunded = 4    // Payment refunded
}
```

**PaymentMethod** (`src/QRDine.Domain/Enums/PaymentMethod.cs`):

```csharp
public enum PaymentMethod
{
    PayOS = 1,          // PayOS gateway
    BankTransfer = 2,   // Direct bank transfer (used for PayOS in system)
    Manual_Admin = 3,   // Admin manual entry
    System_Grant = 4    // Complimentary/system grant
}
```

**SubscriptionStatus** (`src/QRDine.Domain/Enums/SubscriptionStatus.cs`):

```csharp
public enum SubscriptionStatus
{
    Trialing = 1,   // Trial period
    Active = 2,     // Actively paying subscription
    Expired = 3,    // Subscription expired
    Cancelled = 4   // User cancelled
}
```

---

## API Implementation

### 1. CreateCheckoutLinkCommand & Handler

**Command**:

```csharp
public record CreateCheckoutLinkCommand(Guid PlanId) : IRequest<string>;
```

**Handler** (`src/QRDine.Application/Features/Billing/Plans/Commands/CreateCheckoutLink/CreateCheckoutLinkCommandHandler.cs`):

```csharp
public class CreateCheckoutLinkCommandHandler : IRequestHandler<CreateCheckoutLinkCommand, string>
{
    private readonly IPlanRepository _planRepository;
    private readonly ISubscriptionCheckoutRepository _checkoutRepo;
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPayOSService _payOSService;
    private readonly IFrontendConfig _frontendConfig;

    public async Task<string> Handle(CreateCheckoutLinkCommand request, CancellationToken cancellationToken)
    {
        // 1. Get merchant from current user
        var merchantId = _currentUserService.MerchantId
            ?? throw new UnauthorizedAccessException("Could not find merchant information");

        // 2. Load plan
        var plan = await _planRepository.GetByIdAsync(request.PlanId, cancellationToken);
        if (plan == null || !plan.IsActive)
            throw new Exception("Plan does not exist or is no longer available");

        // 3. Generate order code (YYMMDDHHmmssfff)
        var orderCode = long.Parse(DateTimeOffset.UtcNow.ToString("yyMMddHHmmssfff"));

        // 4. Check current subscription to determine action prefix
        var subSpec = new GetSubscriptionByMerchantIdSpec(merchantId);
        var currentSubscription = await _subscriptionRepo.SingleOrDefaultAsync(subSpec, cancellationToken);

        string prefix = "Mua";  // Default: New purchase
        if (currentSubscription != null
            && currentSubscription.Status == SubscriptionStatus.Active
            && currentSubscription.EndDate > DateTime.UtcNow)
        {
            prefix = currentSubscription.PlanId == plan.Id ? "Gia han" : "Nang cap";
        }

        // 5. Create checkout record
        var checkoutRecord = new SubscriptionCheckout
        {
            OrderCode = orderCode,
            MerchantId = merchantId,
            PlanId = plan.Id,
            Amount = plan.Price,
            Status = PaymentStatus.Pending,
            PlanSnapshotName = plan.Name
        };
        await _checkoutRepo.AddAsync(checkoutRecord, cancellationToken);

        // 6. Build description (max 25 chars)
        var shortCode = merchantId.ToString().Substring(0, 6).ToUpper();
        var description = $"{prefix} {plan.Code} {shortCode}";
        if (description.Length > 25)
            description = description.Substring(0, 25).Trim();

        // 7. Create payment link request
        var frontendBaseUrl = _frontendConfig.BaseUrl.TrimEnd('/');
        var paymentData = new PaymentLinkRequestDto
        {
            OrderCode = orderCode,
            Amount = (int)plan.Price,
            Description = description,
            CancelUrl = $"{frontendBaseUrl}/management/billing/cancel",
            ReturnUrl = $"{frontendBaseUrl}/management/billing/success"
        };

        // 8. Get checkout URL from PayOS
        return await _payOSService.CreatePaymentLinkAsync(paymentData);
    }
}
```

**Key Features**:

- Validates merchant authentication
- Checks plan existence and active status
- Generates unique order code based on timestamp
- Determines subscription action (new/renewal/upgrade)
- Creates `SubscriptionCheckout` record for tracking
- Generates description with merchant identifier
- Calls PayOS to create checkout link
- Returns checkout URL to frontend

### 2. ProcessPaymentWebhookCommand & Handler

**Command** (`src/QRDine.Application/Features/Billing/Plans/Commands/ProcessPaymentWebhook/ProcessPaymentWebhookCommand.cs`):

```csharp
public record ProcessPaymentWebhookCommand(long OrderCode, long Amount, string Reference) : IRequest<bool>;
```

**Handler** (`src/QRDine.Application/Features/Billing/Plans/Commands/ProcessPaymentWebhook/ProcessPaymentWebhookCommandHandler.cs`):

```csharp
public class ProcessPaymentWebhookCommandHandler : IRequestHandler<ProcessPaymentWebhookCommand, bool>
{
    private readonly ISubscriptionCheckoutRepository _checkoutRepo;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IApplicationDbContext _context;

    public async Task<bool> Handle(ProcessPaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        // 1. Find checkout record by order code
        var spec = new SubscriptionCheckoutByOrderCodeSpec(request.OrderCode);
        var checkoutRecord = await _checkoutRepo.SingleOrDefaultAsync(spec, cancellationToken);

        if (checkoutRecord == null || checkoutRecord.Status != PaymentStatus.Pending)
            return true;  // Idempotent: ignore already processed

        // 2. Verify received amount
        if (request.Amount < checkoutRecord.Amount)
        {
            checkoutRecord.Status = PaymentStatus.Failed;
            checkoutRecord.FailureReason = $"Customer transferred insufficient funds. " +
                $"Required: {checkoutRecord.Amount}, Received: {request.Amount}";
            await _checkoutRepo.UpdateAsync(checkoutRecord, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        // 3. Begin transaction for data consistency
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        try
        {
            // 4. Assign plan to merchant
            await _subscriptionService.AssignPlanAsync(
                merchantId: checkoutRecord.MerchantId,
                planId: checkoutRecord.PlanId,
                paymentMethod: PaymentMethod.BankTransfer,
                overrideAmount: request.Amount,
                adminNote: $"PayOS Payment. Reference: {request.Reference}",
                cancellationToken: cancellationToken
            );

            // 5. Update checkout status to success
            checkoutRecord.Status = PaymentStatus.Success;
            await _checkoutRepo.UpdateAsync(checkoutRecord, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
```

**Key Features**:

- Idempotent: handles duplicate webhooks safely
- Validates received amount
- Records failure reason for insufficient payments
- Uses database transaction for atomicity
- Assigns subscription via `SubscriptionService`
- Records payment in transaction history
- Rolls back on exception for consistency

---

## Webhook Handling

### 1. PayOSWebhookController

**Location**: `src/QRDine.API/Controllers/Webhooks/PayOSWebhookController.cs`

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/webhooks")]
[ApiExplorerSettings(IgnoreApi = true)]  // Hide from Swagger
public class PayOSWebhookController : ControllerBase
{
    private readonly PayOSClient _payOSClient;
    private readonly IMediator _mediator;
    private readonly ILogger<PayOSWebhookController> _logger;

    public PayOSWebhookController(PayOSClient payOSClient, IMediator mediator, ILogger<PayOSWebhookController> logger)
    {
        _payOSClient = payOSClient;
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("payos")]
    [AllowAnonymous]  // Required for PayOS webhook
    public async Task<IActionResult> HandleWebhook([FromBody] Webhook webhookBody)
    {
        try
        {
            // 1. Verify webhook signature using ChecksumKey
            var webhookData = await _payOSClient.Webhooks.VerifyAsync(webhookBody);

            // 2. Check if payment succeeded (code "00")
            if (webhookData.Code == "00")
            {
                var command = new ProcessPaymentWebhookCommand(
                    webhookData.OrderCode,
                    webhookData.Amount,
                    webhookData.Reference);

                await _mediator.Send(command);
            }

            // 3. Always return 200 OK (idempotent)
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PayOS webhook. Data: {@WebhookBody}", webhookBody);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
```

### 2. Webhook Verification Flow

```
PayOS Server
    │
    ├─ Generate webhook payload
    ├─ Create signature: HMAC(payload, ChecksumKey)
    └─ POST to /api/v1/webhooks/payos
                    │
                    ▼
            PayOSWebhookController
                    │
                    ├─ Receive webhook body
                    ├─ Call PayOSClient.Webhooks.VerifyAsync()
                    │  └─ Verify signature matches ChecksumKey
                    │  └─ Parse webhook data
                    │
                    ├─ Check if code == "00" (success)
                    │
                    ├─ Dispatch ProcessPaymentWebhookCommand if success
                    │
                    └─ Return 200 OK (always)
```

### 3. Webhook Payload Structure

**Request Body** (Webhook object):

```json
{
  "code": "00",
  "desc": "Payment successful",
  "data": {
    "orderCode": 2602212345678,
    "amount": 99000,
    "amountPaid": 99000,
    "amountRemaining": 0,
    "status": "COMPLETED",
    "createDate": 1708605954,
    "transactions": [
      {
        "reference": "TRANSFER2602212345678",
        "amount": 99000,
        "accountBank": "970422",
        "description": "Mua PLN_01 AB1234",
        "transactionDateTime": 1708605954
      }
    ],
    "canceledDate": null,
    "expiredDate": null
  },
  "signature": "payos-checksum-signature-here"
}
```

### 4. Webhook Configuration

**In PayOS Dashboard**:

1. Go to Settings/Webhooks
2. Add webhook URL: `https://api.qrdine.me/api/v1/webhooks/payos`
3. Events to trigger:
   - Payment successful (recommended)
   - Payment cancelled (optional)
   - Payment expired (optional)

**Security**:

- PayOS uses HMAC-SHA256 signature
- Checksum includes all payload data + ChecksumKey
- Signature verification is automatic via PayOSClient SDK
- Only process webhooks with valid signatures

---

## Error Handling

### 1. Payment Creation Errors

| Error                                           | Cause                           | Resolution                         |
| ----------------------------------------------- | ------------------------------- | ---------------------------------- |
| `PayOS configuration is missing`                | Config not in appsettings.json  | Add PayOS section to configuration |
| `Plan does not exist or is no longer available` | Plan not found or inactive      | Select active plan                 |
| `Could not find merchant information`           | User not properly authenticated | Login and ensure merchant context  |
| `HttpRequestException` - Timeout                | PayOS API not responding        | Retry, check PayOS status          |

**Handler Example**:

```csharp
try
{
    var checkoutUrl = await _payOSService.CreatePaymentLinkAsync(paymentData);
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "PayOS API error for order: {OrderCode}", orderCode);
    throw new ApplicationException("Payment service temporarily unavailable. Please try again.");
}
```

### 2. Webhook Processing Errors

| Error                         | Cause                                         | Resolution                             |
| ----------------------------- | --------------------------------------------- | -------------------------------------- |
| Invalid signature             | Webhook tampered or wrong ChecksumKey         | Verify ChecksumKey in config           |
| `OrderCode not found`         | Checkout record deleted or OrderCode mismatch | Idempotent: mark as processed          |
| Insufficient amount           | Customer paid less than required              | Mark as failed, notify customer        |
| Database constraint violation | Duplicate subscription creation               | Database transaction ensures atomicity |

**Handler Example**:

```csharp
// Amount validation
if (request.Amount < checkoutRecord.Amount)
{
    checkoutRecord.Status = PaymentStatus.Failed;
    checkoutRecord.FailureReason = $"Insufficient payment. Required: {checkoutRecord.Amount}, Received: {request.Amount}";
    await _checkoutRepo.UpdateAsync(checkoutRecord, cancellationToken);
    return true;  // Don't throw, log and continue
}
```

### 3. Subscription Assignment Errors

| Error                      | Cause                      | Resolution                                                 |
| -------------------------- | -------------------------- | ---------------------------------------------------------- |
| `NotFoundException`        | Merchant or Plan not found | Validate data before webhook processing                    |
| Transaction rollback       | DB operation failed        | Entire transaction rolls back, checkout stays Pending      |
| Cache invalidation failure | Redis unavailable          | Subscription still works, cache will refresh on next query |

**Transactional Safety**:

```csharp
await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
try
{
    // All operations inside transaction
    await _subscriptionService.AssignPlanAsync(...);
    checkoutRecord.Status = PaymentStatus.Success;
    await _checkoutRepo.UpdateAsync(checkoutRecord, cancellationToken);

    await _context.SaveChangesAsync(cancellationToken);
    await transaction.CommitAsync(cancellationToken);
}
catch (Exception)
{
    await transaction.RollbackAsync(cancellationToken);
    throw;  // Webhook handler logs and returns 500
}
```

### 4. Frontend Error Handling

**Return URLs**:

- `Success`: `/management/billing/success` - Subscription active, show confirmation
- `Cancel`: `/management/billing/cancel` - User cancelled payment

**Polling for Status** (Frontend):

```javascript
// Frontend should poll the subscription status endpoint
const checkSubscription = async () => {
  const response = await fetch("/api/v1/management/billing/subscription");
  return response.json();
};

// Retry until subscription becomes active or timeout
```

---

## Testing

### 1. Unit Tests

**Location**: `tests/QRDine.Application.Tests/Features/Billing/Plans/Commands/ProcessPaymentWebhook/ProcessPaymentWebhookCommandHandlerTests.cs`

**Test Cases**:

```csharp
public class ProcessPaymentWebhookCommandHandlerTests
{
    private readonly Mock<ISubscriptionCheckoutRepository> _checkoutRepo = new();
    private readonly Mock<ISubscriptionService> _subscriptionService = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly ProcessPaymentWebhookCommandHandler _handler;

    // Test 1: Successful payment with new subscription
    [Fact]
    public async Task Handle_WithValidWebhook_AssignsSubscription()
    {
        var orderCode = long.Parse(DateTimeOffset.UtcNow.ToString("yyMMddHHmmssfff"));
        var checkout = new SubscriptionCheckout
        {
            OrderCode = orderCode,
            MerchantId = Guid.NewGuid(),
            PlanId = Guid.NewGuid(),
            Amount = 99000,
            Status = PaymentStatus.Pending
        };

        _checkoutRepo
            .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISpecification<SubscriptionCheckout>>(), default))
            .ReturnsAsync(checkout);

        var command = new ProcessPaymentWebhookCommand(orderCode, 99000, "REF123");
        var result = await _handler.Handle(command, default);

        Assert.True(result);
        Assert.Equal(PaymentStatus.Success, checkout.Status);
    }

    // Test 2: Insufficient payment amount
    [Fact]
    public async Task Handle_WithInsufficientAmount_MarksFailed()
    {
        var checkout = new SubscriptionCheckout
        {
            OrderCode = 123456,
            Amount = 99000,
            Status = PaymentStatus.Pending
        };

        _checkoutRepo
            .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISpecification<SubscriptionCheckout>>(), default))
            .ReturnsAsync(checkout);

        var command = new ProcessPaymentWebhookCommand(123456, 50000, "REF123");
        var result = await _handler.Handle(command, default);

        Assert.True(result);
        Assert.Equal(PaymentStatus.Failed, checkout.Status);
        Assert.NotNull(checkout.FailureReason);
    }

    // Test 3: Idempotent - already processed
    [Fact]
    public async Task Handle_WithAlreadyProcessedCheckout_ReturnsTrue()
    {
        var checkout = new SubscriptionCheckout
        {
            OrderCode = 123456,
            Status = PaymentStatus.Success  // Already success
        };

        _checkoutRepo
            .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISpecification<SubscriptionCheckout>>(), default))
            .ReturnsAsync(checkout);

        var command = new ProcessPaymentWebhookCommand(123456, 99000, "REF123");
        var result = await _handler.Handle(command, default);

        Assert.True(result);
        _subscriptionService.Verify(x => x.AssignPlanAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<PaymentMethod>(), It.IsAny<decimal?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Test 4: Transaction rollback on error
    [Fact]
    public async Task Handle_OnServiceError_RollsbackTransaction()
    {
        var checkout = new SubscriptionCheckout
        {
            OrderCode = 123456,
            Amount = 99000,
            Status = PaymentStatus.Pending
        };

        _checkoutRepo
            .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISpecification<SubscriptionCheckout>>(), default))
            .ReturnsAsync(checkout);

        _subscriptionService
            .Setup(x => x.AssignPlanAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<PaymentMethod>(), It.IsAny<decimal?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        var command = new ProcessPaymentWebhookCommand(123456, 99000, "REF123");

        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, default));

        // Verify rollback was called
        _context.Verify(x => x.BeginTransactionAsync(default), Times.Once);
    }
}
```

### 2. Integration Tests - Webhook Endpoint

```csharp
[Collection("Sequential")]
public class PayOSWebhookIntegrationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _client = default!;

    // Test: Process valid webhook
    [Fact]
    public async Task HandleWebhook_WithValidSignature_ProcessesPayment()
    {
        var webhook = new Webhook
        {
            code = "00",
            data = new { orderCode = 123456, amount = 99000, reference = "REF123" },
            signature = "valid-signature"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/v1/webhooks/payos",
            webhook);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Test: Reject invalid signature
    [Fact]
    public async Task HandleWebhook_WithInvalidSignature_ReturnsBadRequest()
    {
        var webhook = new Webhook
        {
            code = "00",
            data = new { orderCode = 123456, amount = 99000 },
            signature = "invalid-signature"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/v1/webhooks/payos",
            webhook);

        // PayOSClient.VerifyAsync throws, caught and returns BadRequest
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
```

### 3. Manual Testing Checklist

**Scenario 1: Complete Payment Flow**

- [ ] Create plan in database
- [ ] Call `/api/v1/management/billing/plans/checkout` with PlanId
- [ ] Receive checkout URL
- [ ] Redirect to PayOS checkout (test environment)
- [ ] Complete test payment
- [ ] Verify webhook received with valid signature
- [ ] Check subscription created/extended in database
- [ ] Verify frontend redirected to success page

**Scenario 2: Insufficient Payment**

- [ ] Set up webhook to send amount < plan.Price
- [ ] Verify SubscriptionCheckout.Status = Failed
- [ ] Check FailureReason populated correctly
- [ ] Subscription should NOT be created

**Scenario 3: Duplicate Webhook**

- [ ] Send same webhook twice
- [ ] First: Status = Success, subscription created
- [ ] Second: Idempotent, no error, no duplicate subscription

**Scenario 4: Configuration Missing**

- [ ] Remove PayOS section from appsettings.json
- [ ] Application should fail to start with clear error message

---

## Troubleshooting

### Common Issues

#### 1. "PayOS configuration is missing" on Application Start

**Cause**: `PayOS` section not in `appsettings.json` or `appsettings.{Environment}.json`

**Solution**:

```json
{
  "PayOS": {
    "ClientId": "your_value",
    "ApiKey": "your_value",
    "ChecksumKey": "your_value"
  }
}
```

#### 2. Webhook Not Received

**Causes & Solutions**:

| Symptom                        | Check                         | Solution                                   |
| ------------------------------ | ----------------------------- | ------------------------------------------ |
| PayOS webhook never sent       | 1. PayOS dashboard → Webhooks | Verify webhook URL configured              |
| Webhook sent but not processed | 2. API logs                   | Check for `AllowAnonymous` attribute       |
| 500 error in logs              | 3. Exception stack trace      | Check error type - likely DB or validation |
| "Invalid signature" error      | 4. ChecksumKey value          | Regenerate from PayOS dashboard            |

**Debug Webhook URL**:

```bash
# Production URL format
https://api.qrdine.me/api/v1.0/webhooks/payos

# Development URL (ngrok example)
https://d1a2b3c4.ngrok.io/api/v1.0/webhooks/payos
```

#### 3. "Unauthorized" When Creating Checkout

**Cause**: No authentication token or expired token

**Solution**:

```
Header: Authorization: Bearer {jwt_token}
```

#### 4. Payment Link Not Generated

**Cause**: PayOS API timeout or invalid request

**Check**:

```csharp
// Verify PayOS credentials
var payOSClient = new PayOSClient(new PayOSOptions
{
    ClientId = settings.ClientId,        // Must be UUID format
    ApiKey = settings.ApiKey,            // Must be UUID format
    ChecksumKey = settings.ChecksumKey   // Must be 64-char hex string
});

// Check PayOS API status
// https://payos.vn/status
```

#### 5. Subscription Not Created After Payment

**Causes**:

1. **Webhook not processed**: Check logs for webhook handler
2. **Insufficient amount**: Payment received < plan.Price
3. **Plan not found**: Check if plan.IsActive = true
4. **Merchant not found**: Verify merchantId in checkout record

**Debug Query**:

```sql
-- Check checkout record
SELECT * FROM SubscriptionCheckouts WHERE OrderCode = @orderCode;

-- Check subscription created
SELECT * FROM Subscriptions WHERE MerchantId = @merchantId;

-- Check transaction
SELECT * FROM Transactions WHERE ProviderId = @reference;
```

#### 6. Cache Not Invalidating

**Issue**: Merchant sees old subscription status

**Solution**:

```csharp
// Manual cache clear
var cacheKey = CacheKeys.MerchantActiveStatus(merchantId);
await _cacheService.RemoveAsync(cacheKey);

// Or wait for TTL (default: varies by cache implementation)
```

#### 7. Duplicate Payment Links Generated

**Issue**: Multiple checkout URLs for same PlanId

**Expected Behavior**:

- Each call generates new SubscriptionCheckout with new OrderCode
- Multiple payment attempts are allowed
- Only first successful payment counts (transaction atomic)

**Check for Orphaned Checkouts**:

```sql
SELECT * FROM SubscriptionCheckouts
WHERE Status = 'Pending'
AND CreatedAt < DATEADD(day, -1, GETDATE());
```

### Logging Guide

**Enable PayOS Debug Logging**:

```csharp
// In appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "QRDine.Infrastructure.PayOS.PayOSService": "Debug"
```

**Log Levels**:

- **Info**: Payment link created, webhook received
- **Warn**: Insufficient amount, configuration warnings
- **Error**: API failures, unexpected exceptions

**Useful Log Searches**:

```
Error processing PayOS webhook
PayOS configuration is missing
Insufficient payment
Payment service temporarily unavailable
```

---

## Additional Resources

### PayOS API Documentation

- Official Docs: [https://payos.vn/docs/](https://payos.vn/docs/)
- Webhook Sample: [https://payos.vn/docs/webhooks](https://payos.vn/docs/webhooks)
- Error Codes: [https://payos.vn/docs/errors](https://payos.vn/docs/errors)

### QRDine Architecture References

- [Billing Module Overview](../features/billing/overview.md)
- [Subscription Management](../features/billing/subscriptions.md)
- [External Services Integration](README.md)
- [Error Handling](../../architecture/patterns-and-design.md#error-handling)

### Related Files

- Billing Feature Module: `src/QRDine.Application/Features/Billing/`
- Domain Models: `src/QRDine.Domain/Billing/`
- API Controllers: `src/QRDine.API/Controllers/Webhooks/`
- Configuration: `src/QRDine.Infrastructure/Configuration/`

---

## FAQ

**Q: What happens if payment processing takes more than 30 seconds?**
A: HTTP client timeout (30s) is configured in dependency injection. PayOS retry policy on their end ensures webhook delivery within reasonable time.

**Q: Can a merchant have multiple pending checkouts?**
A: Yes, each call creates new checkout with new OrderCode. Only first successful payment activates subscription.

**Q: What if webhook arrives before user returned to frontend?**
A: Subscription is created via webhook immediately. Frontend success page queries subscription API and displays current status.

**Q: How do we handle refunds?**
A: Currently not implemented. Future enhancement: Add refund command that calls PayOS API + updates Transaction.Status to Refunded, extends subscription EndDate compensation.

**Q: Can order code be regenerated if duplicate order occurs?**
A: No - OrderCode is timestamp-based and guaranteed unique within millisecond precision. Duplicate webhooks are handled idempotently by checking checkout.Status.

**Q: Is PCI DSS compliance required?**
A: QRDine doesn't store card data - PayOS handles all payment processing. PCI compliance responsibility is on PayOS. QRDine only stores transaction references.
