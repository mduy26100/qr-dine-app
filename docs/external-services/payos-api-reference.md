# PayOS Payment Integration - API Reference & Examples

## Quick Reference

### Endpoints Summary

| Endpoint                                    | Method | Purpose                 | Auth Required | Notes                        |
| ------------------------------------------- | ------ | ----------------------- | ------------- | ---------------------------- |
| `/api/v1/management/billing/plans/checkout` | POST   | Create payment link     | ✓ JWT         | Returns checkout URL         |
| `/api/v1/webhooks/payos`                    | POST   | Receive payment webhook | ✗ None        | Must be public               |
| `/api/v1/management/billing/subscription`   | GET    | Get subscription info   | ✓ JWT         | Returns current subscription |

---

## 1. Create Checkout Link

### Request

```http
POST /api/v1/management/billing/plans/checkout HTTP/1.1
Host: api.qrdine.me
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json

{
  "planId": "550e8400-e29b-41d4-a716-446655440000"
}
```

### Response (200 OK)

```json
"https://payos.vn/web/a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6?orderCode=2602212345678"
```

### Response (400 Bad Request)

**Missing PlanId**:

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": {
    "planId": "PlanId is required"
  }
}
```

**Plan Not Found**:

```json
{
  "success": false,
  "message": "Plan does not exist or is no longer available"
}
```

### Response (401 Unauthorized)

```json
{
  "success": false,
  "message": "Could not find merchant information"
}
```

### cURL Example

```bash
curl -X POST http://localhost:7288/api/v1/management/billing/plans/checkout \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{"planId":"550e8400-e29b-41d4-a716-446655440000"}'
```

### JavaScript/Fetch Example

```javascript
const createCheckoutLink = async (planId, jwtToken) => {
  try {
    const response = await fetch("/api/v1/management/billing/plans/checkout", {
      method: "POST",
      headers: {
        Authorization: `Bearer ${jwtToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ planId }),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    const checkoutUrl = await response.text();
    return checkoutUrl;
  } catch (error) {
    console.error("Checkout creation failed:", error);
    throw error;
  }
};

// Usage
try {
  const url = await createCheckoutLink(planId, token);
  window.location.href = url; // Redirect to PayOS
} catch (error) {
  // Show error message to user
}
```

### Python Example (Requests)

```python
import requests

def create_checkout_link(plan_id, jwt_token):
    url = "http://localhost:7288/api/v1/management/billing/plans/checkout"
    headers = {
        "Authorization": f"Bearer {jwt_token}",
        "Content-Type": "application/json"
    }
    payload = {"planId": plan_id}

    response = requests.post(url, json=payload, headers=headers)
    response.raise_for_status()

    return response.text  # Returns checkout URL directly

# Usage
checkout_url = create_checkout_link(
    plan_id="550e8400-e29b-41d4-a716-446655440000",
    jwt_token="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
)
print(f"Redirect to: {checkout_url}")
```

---

## 2. Webhook Handler

### Webhook Request (From PayOS)

**POST** `/api/v1/webhooks/payos`

```json
{
  "code": "00",
  "desc": "Successful",
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
        "accountNumber": "1234567890",
        "description": "Mua PLN_01 AB1234",
        "transactionDateTime": 1708605954
      }
    ],
    "cancellationDate": null,
    "expiredDate": null
  },
  "signature": "7f7a3b2c1d9e8f4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7"
}
```

### Webhook Response (200 OK)

**Raw Response**:

```json
{
  "success": true
}
```

### Webhook Responses (Error Cases)

**Invalid Signature (400)**:

```json
{
  "success": false,
  "message": "Signature verification failed"
}
```

**Server Error (500)**:

```json
{
  "success": false,
  "message": "An unexpected error occurred"
}
```

### Webhook Testing with cURL

```bash
# Set up variables
WEBHOOK_URL="http://localhost:7288/api/v1/webhooks/payos"
ORDER_CODE="2602212345678"
AMOUNT="99000"

# Create test webhook payload
WEBHOOK_PAYLOAD='{
  "code": "00",
  "desc": "Webhook test",
  "data": {
    "orderCode": '${ORDER_CODE}',
    "amount": '${AMOUNT}',
    "amountPaid": '${AMOUNT}',
    "status": "COMPLETED",
    "transactions": [{
      "reference": "TEST'${ORDER_CODE}'",
      "amount": '${AMOUNT}'
    }]
  },
  "signature": "test-signature"
}'

# Send webhook
curl -X POST "${WEBHOOK_URL}" \
  -H "Content-Type: application/json" \
  -d "${WEBHOOK_PAYLOAD}" \
  -v
```

### Webhook Retry Behavior

**PayOS Retry Policy**:

- Retry interval: 5, 10, 30, 60 minutes
- Max retries: 6 times over ~3 hours
- Idempotent: Safe to receive same webhook multiple times

**Handler Idempotency**:

```csharp
// Handler checks subscription checkout status
if (checkoutRecord.Status != PaymentStatus.Pending)
{
    return true;  // Already processed, no action needed
}
```

---

## 3. Get Subscription Info

### Request

```http
GET /api/v1/management/billing/subscription HTTP/1.1
Host: api.qrdine.me
Authorization: Bearer {JWT_TOKEN}
```

### Response (200 OK)

**Active Subscription**:

```json
{
  "planCode": "PRO",
  "status": "Active",
  "endDate": "2025-03-30T14:23:45.123Z"
}
```

**Expired Subscription**:

```json
{
  "planCode": "BASIC",
  "status": "Expired",
  "endDate": "2025-02-28T00:00:00.000Z"
}
```

**No Subscription** (HTTP 204 No Content):

```
(empty body)
```

### JavaScript Example

```javascript
const getSubscriptionInfo = async (jwtToken) => {
  const response = await fetch("/api/v1/management/billing/subscription", {
    headers: {
      Authorization: `Bearer ${jwtToken}`,
    },
  });

  if (response.status === 204) {
    return null; // No subscription
  }

  if (!response.ok) {
    throw new Error("Failed to fetch subscription");
  }

  return await response.json();
};

// Usage
const subscription = await getSubscriptionInfo(token);
if (subscription) {
  console.log(`Plan: ${subscription.planCode}`);
  console.log(`Status: ${subscription.status}`);
  console.log(`Expires: ${new Date(subscription.endDate)}`);
}
```

---

## 4. Configuration Setup

### appsettings.json

```json
{
  "PayOS": {
    "ClientId": "<your-client-id>",
    "ApiKey": "<your-api-key>",
    "ChecksumKey": "<your-checksum-key>"
  },
  "FrontendSettings": {
    "BaseUrl": "http://localhost:5173"
  }
}
```

### Environment Variables

```bash
# .env or docker/.env
PAYOS_CLIENT_ID=<your-client-id>
PAYOS_API_KEY=<your-api-key>
PAYOS_CHECKSUM_KEY=<your-checksum-key>
FRONTEND_BASE_URL=http://localhost:5173
```

### appsettings.Production.json (for production)

```json
{
  "PayOS": {
    "ClientId": "{{PAYOS_CLIENT_ID}}",
    "ApiKey": "{{PAYOS_API_KEY}}",
    "ChecksumKey": "{{PAYOS_CHECKSUM_KEY}}"
  },
  "FrontendSettings": {
    "BaseUrl": "https://qr-dine-ui.vercel.app"
  }
}
```

---

## 5. Frontend Integration Examples

### React Component Example

```typescript
import React, { useState, useEffect } from 'react';

interface SubscriptionInfo {
  planCode: string;
  status: 'Active' | 'Expired' | 'Trialing' | 'Cancelled';
  endDate: string;
}

export const BillingPage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [subscription, setSubscription] = useState<SubscriptionInfo | null>(null);

  useEffect(() => {
    fetchSubscription();
  }, []);

  const fetchSubscription = async () => {
    const response = await fetch('/api/v1/management/billing/subscription', {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    });

    if (response.status === 204) {
      setSubscription(null);
    } else if (response.ok) {
      const data = await response.json();
      setSubscription(data);
    }
  };

  const handleUpgrade = async (planId: string) => {
    setLoading(true);
    try {
      const response = await fetch('/api/v1/management/billing/plans/checkout', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ planId })
      });

      if (!response.ok) {
        throw new Error('Failed to create checkout link');
      }

      const checkoutUrl = await response.text();
      window.location.href = checkoutUrl;  // Redirect to PayOS
    } catch (error) {
      alert(`Error: ${error.message}`);
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN');
  };

  return (
    <div className="billing-container">
      <h1>Gói cước của bạn</h1>

      {subscription ? (
        <div className="subscription-info">
          <p>Gói: <strong>{subscription.planCode}</strong></p>
          <p>Trạng thái: <strong>{subscription.status}</strong></p>
          <p>Hết hạn: <strong>{formatDate(subscription.endDate)}</strong></p>

          {subscription.status === 'Active' && (
            <button onClick={() => handleUpgrade('pro-plan-id')} disabled={loading}>
              {loading ? 'Đang xử lý...' : 'Nâng cấp'}
            </button>
          )}

          {(subscription.status === 'Expired' || subscription.status === 'Cancelled') && (
            <button onClick={() => handleUpgrade('basic-plan-id')} disabled={loading}>
              {loading ? 'Đang xử lý...' : 'Gia hạn gói'}
            </button>
          )}
        </div>
      ) : (
        <div className="no-subscription">
          <p>Bạn chưa có gói cước nào</p>
          <button onClick={() => handleUpgrade('basic-plan-id')} disabled={loading}>
            {loading ? 'Đang xử lý...' : 'Chọn gói'}
          </button>
        </div>
      )}

      <div className="return-handlers">
        <h2>Trạng thái thanh toán</h2>
        {window.location.pathname === '/management/billing/success' && (
          <div className="success-message">
            ✓ Thanh toán thành công! Gói cước của bạn đã kích hoạt.
          </div>
        )}
        {window.location.pathname === '/management/billing/cancel' && (
          <div className="cancel-message">
            ✗ Đã hủy thanh toán. Gói cước không thay đổi.
          </div>
        )}
      </div>
    </div>
  );
};
```

### Vue.js Component Example

```vue
<template>
  <div class="billing-page">
    <h1>Gói cước của bạn</h1>

    <div v-if="subscription" class="subscription-info">
      <p>
        Gói: <strong>{{ subscription.planCode }}</strong>
      </p>
      <p>
        Trạng thái: <strong>{{ subscription.status }}</strong>
      </p>
      <p>
        Hết hạn: <strong>{{ formatDate(subscription.endDate) }}</strong>
      </p>

      <button
        v-if="subscription.status === 'Active'"
        @click="handleUpgrade('pro-plan-id')"
        :disabled="loading"
      >
        {{ loading ? "Đang xử lý..." : "Nâng cấp" }}
      </button>
      <button
        v-else
        @click="handleUpgrade('basic-plan-id')"
        :disabled="loading"
      >
        {{ loading ? "Đang xử lý..." : "Gia hạn gói" }}
      </button>
    </div>

    <div v-else class="no-subscription">
      <p>Bạn chưa có gói cước nào</p>
      <button @click="handleUpgrade('basic-plan-id')" :disabled="loading">
        {{ loading ? "Đang xử lý..." : "Chọn gói" }}
      </button>
    </div>

    <div v-if="successMessage" class="success-message">
      ✓ {{ successMessage }}
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";

const subscription = ref(null);
const loading = ref(false);
const successMessage = ref("");
const router = useRouter();

onMounted(async () => {
  await fetchSubscription();

  // Check for success/cancel redirect
  if (router.currentRoute.value.query.success === "true") {
    successMessage.value =
      "Thanh toán thành công! Gói cước của bạn đã kích hoạt.";
  }
});

const fetchSubscription = async () => {
  try {
    const response = await fetch("/api/v1/management/billing/subscription", {
      headers: {
        Authorization: `Bearer ${localStorage.getItem("token")}`,
      },
    });

    if (response.status === 204) {
      subscription.value = null;
    } else if (response.ok) {
      subscription.value = await response.json();
    }
  } catch (error) {
    console.error("Failed to fetch subscription:", error);
  }
};

const handleUpgrade = async (planId) => {
  loading.value = true;
  try {
    const response = await fetch("/api/v1/management/billing/plans/checkout", {
      method: "POST",
      headers: {
        Authorization: `Bearer ${localStorage.getItem("token")}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ planId }),
    });

    if (!response.ok) throw new Error("Failed to create checkout");

    const checkoutUrl = await response.text();
    window.location.href = checkoutUrl;
  } catch (error) {
    alert(`Lỗi: ${error.message}`);
  } finally {
    loading.value = false;
  }
};

const formatDate = (dateString) => {
  return new Date(dateString).toLocaleDateString("vi-VN");
};
</script>

<style scoped>
.billing-page {
  padding: 20px;
}

.subscription-info,
.no-subscription {
  margin: 20px 0;
  padding: 15px;
  border: 1px solid #ddd;
  border-radius: 8px;
}

button {
  padding: 10px 20px;
  margin-top: 10px;
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

button:disabled {
  background-color: #ccc;
  cursor: not-allowed;
}

.success-message {
  margin-top: 20px;
  padding: 15px;
  background-color: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
  border-radius: 4px;
}
</style>
```

---

## 6. Testing Guide

### Postman Collection

```json
{
  "info": {
    "name": "PayOS Integration Tests",
    "description": "QRDine PayOS payment integration test collection"
  },
  "item": [
    {
      "name": "Create Checkout Link",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{jwt_token}}"
          },
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "url": {
          "raw": "{{base_url}}/api/v1/management/billing/plans/checkout",
          "host": ["{{base_url}}"],
          "path": ["api", "v1", "management", "billing", "plans", "checkout"]
        },
        "body": {
          "mode": "raw",
          "raw": "{\n  \"planId\": \"{{plan_id}}\"\n}"
        }
      },
      "response": []
    },
    {
      "name": "Get Subscription Info",
      "request": {
        "method": "GET",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{jwt_token}}"
          }
        ],
        "url": {
          "raw": "{{base_url}}/api/v1/management/billing/subscription",
          "host": ["{{base_url}}"],
          "path": ["api", "v1", "management", "billing", "subscription"]
        }
      },
      "response": []
    },
    {
      "name": "Webhook - Payment Success",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "url": {
          "raw": "{{base_url}}/api/v1/webhooks/payos",
          "host": ["{{base_url}}"],
          "path": ["api", "v1", "webhooks", "payos"]
        },
        "body": {
          "mode": "raw",
          "raw": "{\n  \"code\": \"00\",\n  \"desc\": \"Test webhook\",\n  \"data\": {\n    \"orderCode\": {{order_code}},\n    \"amount\": 99000,\n    \"amountPaid\": 99000,\n    \"status\": \"COMPLETED\",\n    \"transactions\": [{\n      \"reference\": \"TEST{{order_code}}\",\n      \"amount\": 99000\n    }]\n  },\n  \"signature\": \"test-signature\"\n}"
        }
      },
      "response": []
    }
  ],
  "variable": [
    {
      "key": "base_url",
      "value": "http://localhost:7288"
    },
    {
      "key": "jwt_token",
      "value": ""
    },
    {
      "key": "plan_id",
      "value": "550e8400-e29b-41d4-a716-446655440000"
    },
    {
      "key": "order_code",
      "value": "2602212345678"
    }
  ]
}
```

### Integration Test Script (C#)

```csharp
[Collection("Integration")]
public class PayOSIntegrationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IServiceScope _scope;
    private HttpClient _httpClient;
    private string _jwtToken;
    private Guid _merchantId;
    private Guid _planId;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();
        _httpClient = _factory.CreateClient();

        // Setup test data
        _scope = _factory.Services.CreateScope();
        var context = _scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // Create merchant
        _merchantId = Guid.NewGuid();
        // ... create merchant in DB ...

        // Create plan
        _planId = Guid.NewGuid();
        var plan = new Plan
        {
            Id = _planId,
            Code = "TEST_PLAN",
            Name = "Test Plan",
            Price = 99000,
            DurationDays = 30,
            IsActive = true
        };
        context.Plans.Add(plan);
        await context.SaveChangesAsync();

        // Generate JWT token
        _jwtToken = GenerateTestJWT(_merchantId);
    }

    [Fact]
    public async Task CreateCheckoutLink_WithValidData_ReturnsCheckoutUrl()
    {
        var request = new CreateCheckoutLinkCommand(_planId);
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _jwtToken);

        var response = await _httpClient.PostAsync(
            "/api/v1/management/billing/plans/checkout",
            content
        );

        Assert.True(response.IsSuccessStatusCode);
        var checkoutUrl = await response.Content.ReadAsStringAsync();
        Assert.StartsWith("https://payos.vn/", checkoutUrl);
    }

    [Fact]
    public async Task ProcessPaymentWebhook_WithValidSignature_CreatesSubscription()
    {
        // First: Create checkout
        var orderCode = long.Parse(DateTimeOffset.UtcNow.ToString("yyMMddHHmmssfff"));
        var checkout = new SubscriptionCheckout
        {
            OrderCode = orderCode,
            MerchantId = _merchantId,
            PlanId = _planId,
            Amount = 99000,
            Status = PaymentStatus.Pending
        };
        // ... add to DB ...

        // Then: Send webhook
        var webhook = new Webhook
        {
            code = "00",
            data = new { orderCode = orderCode, amount = 99000 },
            signature = "valid-sig"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(webhook),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(
            "/api/v1/webhooks/payos",
            content
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify subscription created
        var subscription = context.Subscriptions.FirstOrDefault(
            s => s.MerchantId == _merchantId
        );
        Assert.NotNull(subscription);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
    }

    public async Task DisposeAsync()
    {
        _scope?.Dispose();
        _factory?.Dispose();
        _httpClient?.Dispose();
    }

    private string GenerateTestJWT(Guid merchantId)
    {
        // Generate JWT token for testing
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("secret_key_for_testing_purposes_min_32_chars");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("merchantId", merchantId.ToString()),
                new Claim(ClaimTypes.Role, "Merchant")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
```

---

## 7. Monitoring & Logging

### Key Metrics to Monitor

```csharp
public class PayOSMetrics
{
    public metric CheckoutLinksCreated { get; set; }      // Counter
    public metric WebhooksReceived { get; set; }          // Counter
    public metric WebhooksProcessed { get; set; }         // Counter
    public metric WebhooksFailed { get; set; }            // Counter
    public metric PaymentsSuccessful { get; set; }        // Counter
    public metric PaymentsFailed { get; set; }            // Counter
    public metric CheckoutLinkCreationTime { get; set; }  // Histogram (ms)
    public metric WebhookProcessingTime { get; set; }     // Histogram (ms)
}
```

### Logging Best Practices

```csharp
// Good: Informative log with context
_logger.LogInformation(
    "PayOS checkout link created. OrderCode: {OrderCode}, " +
    "MerchantId: {MerchantId}, PlanId: {PlanId}, Amount: {Amount}",
    orderCode, merchantId, planId, amount
);

// Good: Error logging with exception details
_logger.LogError(ex,
    "PayOS API error during checkout link creation. " +
    "MerchantId: {MerchantId}, StatusCode: {StatusCode}",
    merchantId, ex.StatusCode
);

// Good: Webhook processing with idempotency tracking
_logger.LogInformation(
    "Processing PayOS webhook. OrderCode: {OrderCode}, " +
    "CurrentStatus: {CurrentStatus}, IsIdempotent: {IsIdempotent}",
    orderCode, currentStatus, isIdempotent
);
```

### Alert Rules (Example)

```sql
-- Alert if webhook processing takes too long
SELECT *
FROM WebhookProcessingTimes
WHERE duration_ms > 5000
GROUP BY hour;

-- Alert on payment failure spike
SELECT COUNT(*) as failed_count
FROM Transactions
WHERE status = 'Failed'
AND paid_at > DATEADD(hour, -1, GETDATE())
HAVING COUNT(*) > 10;

-- Alert on webhook verification failures
SELECT *
FROM Logs
WHERE message LIKE '%Invalid signature%'
AND timestamp > DATEADD(minute, -5, GETDATE());
```

---

## 8. Troubleshooting Common Errors

### Error: "Signature verification failed"

**Cause**: ChecksumKey mismatch or payload corruption

**Debug**:

```csharp
_logger.LogDebug("Webhook body: {@Webhook}", webhookBody);
_logger.LogDebug("ChecksumKey: {ChecksumKey}", settings.ChecksumKey);
```

**Solution**: Regenerate ChecksumKey from PayOS dashboard and update configuration.

### Error: "Payment link creation timeout"

**Cause**: PayOS API unresponsive

**Debug**:

```csharp
try
{
    var paymentLink = await _payOSClient.PaymentRequests.CreateAsync(paymentData);
}
catch (HttpRequestException ex) when (ex.InnerException is TimeoutException)
{
    _logger.LogError("PayOS API timeout after 30 seconds");
    throw new ApplicationException("Payment service unavailable. Please retry.");
}
```

**Solution**: Retry with exponential backoff, check PayOS status page.

### Error: "Insufficient amount received"

**Cause**: Customer paid less than required

**Debug**:

```csharp
_logger.LogWarning(
    "Insufficient payment. Required: {Required}, Received: {Received}, " +
    "Shortfall: {Shortfall}",
    checkoutRecord.Amount, request.Amount, checkoutRecord.Amount - request.Amount
);
```

**Solution**: Notify customer to send additional payment or refund.

---

## Summary

This guide provides:

- Complete API endpoint documentation
- Real-world code examples (JavaScript, Python, C#, Vue, React)
- Configuration setup
- Testing approaches
- Monitoring and logging
- Common error solutions

For more details, see [PayOS Integration Main Documentation](payos-integration.md).
