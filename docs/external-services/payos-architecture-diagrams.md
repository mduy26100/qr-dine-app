# PayOS Integration - Architecture & Flow Diagrams

## Application Architecture

### Layered Architecture with PayOS

```
┌─────────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                          │
├─────────────────────────────────────────────────────────────────┤
│  PayOSWebhookController (POST /webhooks/payos)                 │
│  BillingPlansController (POST /plans/checkout)                 │
└────────────────┬──────────────────────────────────────────┬────┘
                 │                                           │
        ┌────────▼──────────┐                         ┌──────▼───────┐
        │ WEBHOOK FLOW      │                         │ CHECKOUT     │
        │ (Inbound)         │                         │ FLOW         │
        │                   │                         │ (Outbound)   │
        └────────┬──────────┘                         └──────┬───────┘
                 │                                           │
┌────────────────▼───────────────────────────────────────────▼────┐
│                 APPLICATION LAYER (MediatR)                     │
├─────────────────────────────────────────────────────────────────┤
│  ProcessPaymentWebhookCommand                                   │
│  CreateCheckoutLinkCommand                                      │
│  AssignPlanCommand                                              │
└────────────┬────────────────────────────────────────┬───────────┘
             │                                        │
  ┌──────────▼─────────────┐                ┌────────▼────────────┐
  │ DOMAIN LAYER           │                │ INFRASTRUCTURE      │
  │ - Subscription         │                │ - PayOSService      │
  │ - SubscriptionCheckout │                │ - PayOSClient (SDK) │
  │ - Plan                 │                │ - IPayOSService     │
  │ - Transaction          │                │ - SubscriptionSvc   │
  └──────────┬─────────────┘                └────────┬────────────┘
             │                                        │
             └─────────┬──────────────────────────────┘
                       │
           ┌───────────▼───────────┐
           │ DATABASE (DbContext)  │
           │ - Subscriptions       │
           │ - Transactions        │
           │ - Plans               │
           │ - Checkouts           │
           └───────────────────────┘
```

## Payment Processing Flow Sequence Diagram

```
Merchant          Frontend         PayOS Server        QRDine API
   │                 │                  │                   │
   ├─ Click "Upgrade" Plan            │                   │
   │                 │                  │                   │
   ├─────────────────────────────────────────────────────────│─ POST /plans/checkout
   │                 │                  │                   │ (PlanId)
   │                 │                  │                   │
   │                 │                  │          [Authenticate Merchant]
   │                 │                  │          [Load Plan]
   │                 │            [Generate OrderCode]
   │                 │          [Create SubscriptionCheckout - Pending]
   │                 │          [Call PayOS API]◄──────────────┤
   │                 │                  │          [CreatePaymentLink]
   │                 │          [Return CheckoutUrl]
   │                 │                  │<─────────────────│
   │                 │◄───────── CheckoutUrl────────────────│
   │                 │                  │                   │
   ├ Redirect ──────────────────────────▶ PayOS UI          │
   │  to CheckoutUrl                     │                  │
   │                 │                   │                  │
   ├ Select Payment                      │                  │
   │ Method ──────────────────────────────│─ Complete        │
   │                 │                   │                  │
   │                 │    ┌──────────────────────────────┐  │
   │                 │    │ PayOS verifies payment       │  │
   │                 │    │ - Amount validated           │  │
   │                 │    │ - Bank transfer complete     │  │
   │                 │    └──────────────────────────────┘  │
   │                 │                   │                  │
   │                 │            [Send Webhook]            │
   │                 │               Signature verified     │
   │◄────Redirect────┼──────payos?code=success─────────────│ POST /webhooks/payos
   │ (success=ok)    │                   │                  │
   │                 │                   │          [Verify Webhook Signature]
   │                 │                   │          [Extract OrderCode, Amount]
   │                 │                   │          [Find SubscriptionCheckout]
   │                 │                   │          [BEGIN TRANSACTION]
   │                 │                   │          [AssignPlan]
   │                 │                   │            - Create/Extend Subscription
   │                 │                   │            - Record Transaction
   │                 │                   │            - Invalidate Cache
   │                 │                   │          [Update Status → Success]
   │                 │                   │          [COMMIT TRANSACTION]
   │                 │                   │◄────────[Return 200 OK]
   │                 │                   │          └─────────────────────────┘
   ├─ Poll Subscription Status           │                   │
   │ ─────────────────────────────────────────────────────────│─ GET /billing/info
   │                 │                   │                   │
   │                 │                   │          [Query Subscription]
   │                 │                   │          [Return Active Status]
   │◄───Active✓──────┼───────────────────────────────────────│
   │                 │                   │                   │
   ├─ Show Success                       │                   │
   │ Message         │                   │                   │
```

## Data Model Relationships

```
┌──────────────────────────────┐
│          Plan                │
├──────────────────────────────┤
│ Id (PK)                      │
│ Code: string (PLN_PRO)       │
│ Name: string (Pro Plan)      │
│ Price: decimal (99000)       │
│ DurationDays: int (30)       │
│ IsActive: bool               │
│ FeatureLimit (1:1)           │
└──────┬───────────────────────┘
       │
       │ 1:N
       │
  ┌────┴───────────────────────────────────────────┐
  │                                                 │
  ▼                                                 ▼
┌──────────────────────────────┐    ┌──────────────────────────────┐
│    Subscription              │    │   SubscriptionCheckout       │
├──────────────────────────────┤    ├──────────────────────────────┤
│ Id (PK)                      │    │ Id (PK)                      │
│ MerchantId (FK) ─────────────┼──┬─├ MerchantId (FK)              │
│ PlanId (FK) ────┐            │  │ │ PlanId (FK)                  │
│ Status: enum    │            │  │ │ OrderCode: long (unique)     │
│ StartDate       │            │  │ │ Amount: decimal              │
│ EndDate         │            │  │ │ Status: PaymentStatus        │
│ AdminNote       │            │  │ │ FailureReason: string?       │
│                 │            │  │ │ PlanSnapshotName: string?    │
└────┬────────────┼────────────┘  │ │                              │
     │            │               │ │ Derived from Plan at          │
     │            │               │ │ checkout time (immutable)    │
     │ 1:N        │               │ └──────────────────────────────┘
     │            │               │
     │            │               │ 1 (most recent for merchant)
     │            │               │
     ▼            │               │
┌──────────────────────────────┐  │
│      Transaction             │  │
├──────────────────────────────┤  │
│ Id (PK)                      │  │
│ SubscriptionId (FK) ─────────┼──┴─ Links webhook to subscription
│ MerchantId (FK)              │
│ PlanId (FK)                  │
│ Amount: decimal (paid)       │
│ Method: PaymentMethod        │
│ Status: PaymentStatus        │
│ PaidAt: DateTime             │
│ AdminNote: string?           │
│ ProviderReference: string?   │
│ PlanSnapshotName: string?    │
└──────────────────────────────┘

Legend:
PK = Primary Key
FK = Foreign Key
1:N = One-to-Many
→ Navigation Property
```

## Payment Status State Machine

```
                    ┌─────────────────┐
                    │   PENDING       │
                    │ (Initial State) │
                    └────────┬────────┘
                             │
             Webhook received with valid signature
                             │
                ┌────────────┴────────────┐
                │                         │
        [Amount Check]                    │
                │                         │
        ┌───────┴────────┐                │
        │                │                │
   ✓ Amount ≥         ✗ Amount <         │
      Expected          Expected         │
        │                │                │
        │                └──────┐         │
        │                       │         │
        ▼                       ▼         │
    ┌─────────┐          ┌─────────┐     │
    │ SUCCESS │          │ FAILED  │     │
    │ (Payment │         │(Reasons)│     │
    │ completed)        │-Insuff. │     │
    └────┬────┘         │  amt.  │     │
         │              │-Invalid │     │
         │              │  order  │     │
         │              └────┬────┘     │
         │                   │          │
         │      ┌────────────┴─────┐    │
         │      │                  │    │
         │      └──────────────────┘    │
         │                              │
         ▼                              ▼
    ┌──────────┐                   ┌─────────┐
    │Subscription              │   │NO Action│
    │ Created/Extended         │   │(Pending)│
    │ Transaction: Success     │   │         │
    │ Cache Invalidated        │   │         │
    └──────────┘                   └─────────┘
         │                              │
         └──────────────────┬───────────┘
                            │
                  [Webhook Response]
                    Always 200 OK
```

## Error Handling Flow

```
PayOS Webhook Received
    │
    ▼
[Verify Signature]
    │
    ├─ Invalid Signature ──► 400 BadRequest (logged)
    │
    ▼
[Check Code == "00"]
    │
    ├─ code ≠ "00" ──► 200 OK (silently ignored)
    │
    ▼
[Find SubscriptionCheckout]
    │
    ├─ Not Found ──► Return True (idempotent)
    │
    ├─ Status ≠ Pending ──► Return True (already processed)
    │
    ▼
[Verify Amount]
    │
    ├─ Insufficient ──► Mark FAILED + Return True
    │
    ▼
[BEGIN TRANSACTION]
    │
    ├─ Merchant Not Found ──► ROLLBACK + Throw
    │
    ├─ Plan Not Found ──► ROLLBACK + Throw
    │
    ├─ AssignPlan Error ──► ROLLBACK + Throw
    │
    ├─ DB Error ──► ROLLBACK + Throw
    │
    ├─ Cache Error ──► COMMIT anyway (non-critical)
    │
    ▼
[COMMIT TRANSACTION]
    │
    ▼
200 OK Response
```

## Configuration Dependencies

```
┌─────────────────────────────────┐
│     Program.cs / Startup        │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│  AddApplicationServices()        │
├─────────────────────────────────┤
│  ├─ AddInfrastructure()         │
│  │  └─ AddPayOS()  ◄────────────┼─────────┐
│  │     ├─ Load PayOsSettings    │         │
│  │     ├─ Create HttpClient     │         │
│  │     ├─ Init PayOSClient      │         │
│  │     ├─ Register Singleton    │         │
│  │     └─ Register IPayOSService│         │
│  │                              │         │
│  ├─ AddApplication()            │         │
│  │  └─ MediatR Commands         │         │
│  │                              │         │
│  └─ AddPresentation()           │         │
│     └─ FrontendConfig (IFrontendConfig)   │
└─────────────────────────────────┘
             ▲
             │ Configuration from:
             │
    ┌────────┼────────────┐
    │        │            │
    │        │            │
┌───┴──┐  ┌──┴──┐   ┌─────┴──┐
│appsettings.json
│ {                 │   │.env   │
│  "PayOS": {       │   │       │
│   "ClientId": ..  │   │PAYOS_ │
│   "ApiKey": ..    │   │*      │
│   "ChecksumKey":..│   │       │
│  }                │   │       │
│ }                 │   │       │
└─────────┐   └──────┐   └──────┘
          │         │
          └────┬────┘
               │
        PayOsSettings
        (Config class)
```

## Webhook Signature Verification

```
PayOS Server
│
├─ Generate Payload JSON
│  {
│    "orderCode": 123456,
│    "amount": 99000,
│    "status": "COMPLETED",
│    ...
│  }
│
├─ Serialize to string
│  "orderCode=123456&amount=99000..."
│
├─ HMAC-SHA256 with ChecksumKey
│  signature = HMAC-SHA256(payload, ChecksumKey)
│              = "a1b2c3d4e5f6..."
│
├─ Send POST request
│  Body: {
│    "data": { payload json },
│    "signature": "a1b2c3d4e5f6..."
│  }
│
└─► QRDine API
    │
    ├─ Receive webhook body
    │
    ├─ Call PayOSClient.Webhooks.VerifyAsync(webhookBody)
    │  │
    │  ├─ Extract signature from body
    │  │
    │  ├─ Serialize payload same way
    │  │
    │  ├─ Compute HMAC-SHA256 with ChecksumKey
    │  │  computed_sig = "a1b2c3d4e5f6..."
    │  │
    │  ├─ Compare: signature == computed_sig?
    │  │  ✓ Match  → Valid, continue
    │  │  ✗ Mismatch → Invalid, throw exception
    │  │
    │  └─ Return verified data
    │
    └─ Process payment
```

## Fee & Amount Flow

```
Customer Initiates Payment
│
├─ Plan Price: 99,000 VND
│  (Defined in Plan entity)
│
├─ CreateCheckout
│  ├─ Amount = 99,000
│  ├─ OrderCode = 2602212345678
│  └─ Status = Pending
│
├─ PayOS CreatePaymentLink
│  └─ Amount: 99000 (VND, integer)
│
├─ Customer Completes Payment
│  └─ Received Amount: 99,000 - [Bank Fee] = Net
│
├─ PayOS Webhook
│  ├─ Amount: 99,000 (gross amount received)
│  └─ Status: "COMPLETED"
│
├─ Validation
│  ├─ if Amount < checkoutRecord.Amount
│  │  └─ FAILED (insufficient)
│  │
│  └─ Else
│     ├─ Status = SUCCESS
│     └─ Amount stored in Transaction (99,000)
│
└─ Note:
   - QRDine does NOT handle bank fees
   - PayOS deducts fees on their side
   - QRDine records full gross amount
   - Settlement: PayOS handles separately
```

## Multi-Plan Subscription Logic

```
Scenario 1: New Customer (No Subscription)
┌─────────────────────────────────────────────────┐
│ POST /plans/checkout (PlanId=PRO)               │
├─────────────────────────────────────────────────┤
│ Create SubscriptionCheckout {                   │
│   OrderCode: 123,                               │
│   Amount: 99,000,                               │
│   Status: Pending                               │
│ }                                               │
│                                                 │
│ Webhook Received (Success)                      │
│                                                 │
│ Create New Subscription {                       │
│   MerchantId,                                   │
│   PlanId: PRO,                                  │
│   Status: Active,                               │
│   StartDate: now,                               │
│   EndDate: now + 30 days                        │
│ }                                               │
└─────────────────────────────────────────────────┘


Scenario 2: Renewal (Same Plan)
┌─────────────────────────────────────────────────┐
│ Existing: Subscription (PRO, Expired 3d ago)    │
│                                                 │
│ POST /plans/checkout (PlanId=PRO)               │
├─────────────────────────────────────────────────┤
│ Description: "Gia han PRO AB1234"               │
│ (Renewal detected - same plan)                  │
│                                                 │
│ Webhook Received (Success)                      │
│                                                 │
│ Update Existing Subscription {                  │
│   EndDate: max(current_endDate, now) + 30 days │
│   Status: Active                                │
│ }                                               │
│                                                 │
│ Result: 30 more days added to current plan      │
└─────────────────────────────────────────────────┘


Scenario 3: Upgrade (Different Plan)
┌─────────────────────────────────────────────────┐
│ Existing: Subscription (BASIC until 2025-03-30) │
│                                                 │
│ POST /plans/checkout (PlanId=PRO)               │
├─────────────────────────────────────────────────┤
│ Description: "Nang cap PRO AB1234"              │
│ (Upgrade detected - different plan)             │
│                                                 │
│ Webhook Received (Success)                      │
│                                                 │
│ Update Existing Subscription {                  │
│   PlanId: PRO (changed from BASIC),             │
│   StartDate: now (reset),                       │
│   EndDate: now + 30 days,                       │
│   Status: Active                                │
│ }                                               │
│                                                 │
│ Result: Switched to new plan, loses remaining   │
│         days of old plan (immediate effect)     │
└─────────────────────────────────────────────────┘
```

## Cache Invalidation

```
Subscription Cache Keys
├─ MerchantActiveStatus(merchantId)
│  ├─ Stores: bool (is subscription active?)
│  ├─ Used by: StorefrontSubscriptionMiddleware
│  ├─ Used by: SubscriptionEnforcementMiddleware
│  └─ TTL: Configured (typically 1 hour)
│
└─ On Payment Success:
   └─ Remove MerchantActiveStatus cache
      └─ Next query will check DB
      └─ Subscription status reflects immediately
```

## Integration Points

```
External Systems:
┌─────────────┐     ┌─────────────┐     ┌──────────────┐
│   PayOS     │     │  Frontend   │     │  QRDine DB   │
└────┬────────┘     └────┬────────┘     └──────┬───────┘
     │                   │                     │
     │ 1. Create Link    │                     │
     │◄─ OrderCode ──────┤                     │
     │   CheckoutUrl     │                     │
     │                   │                     │
     │                   │ 2. Redirect         │
     │                   │◄──CheckoutUrl───    │
     │ 3. Accept Payment │                     │
     │                   │                     │
     │ 4. Send Webhook   │ 5. Redirect Success │
     │──────────────────────────────────────────│
     │   OrderCode       │    (Background)     │
     │   Amount          │                ┌────┴─────────┬──────┐
     │   Reference       │                │              │      │
     │   Signature       │           [Verify Sig]   [Create Sub] [Record Txn]
     │                   │                │              │      │
     │ 6. Response (200) │          [Check Amount] [Invalidate]  │
     │◄──────────────────────────────────────────────────────────│
     │   {success:true}  │                     │
     │                   │ 7. Poll Status ────────────────────────│
     │                   │    Return Active                      │
     │                   │◄──────────────────────────────────────│
     │                   │                     │
     │                   ├─ Show Success       │
     │                   │                     │
```
