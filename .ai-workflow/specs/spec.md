# Feature Specification: Add Payment Gateway Integration (Stripe Elements)

## Executive Summary & Boundary

This specification defines the technical architecture, data contracts, and integration mechanics for integrating an online payment gateway into the checkout flow. Following human developer confirmation and business direction, all previously unresolved vendor and business decisions are finalized:
- **Payment Provider:** **Stripe** is selected as the sole payment processor for this release, utilizing **Stripe Elements** with the **PaymentIntents API** to minimize PCI-DSS compliance scope (SAQ A / SAQ A-EP). Alternative gateways (PayPal, Adyen) are deferred.
- **Merchant Model & Commission Rates:** The platform operates as a standard single-merchant model. Platform commissions and merchant payouts are calculated and handled asynchronously by internal accounting processes; payment splits (e.g., Stripe Connect) are excluded from the payment gateway critical path.
- **Refund Policy & Mechanism:** Full and partial refunds are formally supported via a protected backend endpoint (`POST /api/payments/refund`) and reconciled via Stripe webhooks (`charge.refunded`).
- **Webhook Reconciliation:** Asynchronous order fulfillment and status transitions are driven by cryptographically validated webhooks listening for `payment_intent.succeeded` and `payment_intent.payment_failed`.

---

## User Story & Developer Guidance

### User Story
As a customer completing a checkout order,  
I want to securely enter my payment details via Stripe Elements,  
So that my payment is processed immediately and my order is confirmed without the merchant handling raw credit card data.

### Resolution of Previous Reviewer Critiques & Open Questions
1. **Payment Provider Selection:** Resolved. Stripe is chosen for its hosted Elements interface, mature TypeScript SDK support, robust testing environment, and low PCI compliance scope.
2. **Commission Rate Strategy:** Resolved. Single-merchant billing model where customer charges go entirely to the primary merchant account. Commission accounting is deferred to internal financial reporting systems.
3. **Refund Policy:** Resolved. Full and partial refunds are supported via an administrative endpoint (`POST /api/payments/refund`) with audit logging and Stripe webhook synchronization.

---

## Domain Models & Database Persistence

The TypeScript backend persists payment records, intent tracking, and refund transactions using a relational database (PostgreSQL/SQLite via an ORM such as Prisma or TypeORM).

### Entity Schemas

#### 1. Payment (`payments`)
Tracks the overall lifecycle of a payment associated with an order.

| Column | Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | `UUID` | Primary Key, Default: `gen_random_uuid()` | Internal payment identifier |
| `order_id` | `VARCHAR(64)` | Indexed, Not Null, Unique | Associated checkout order identifier |
| `customer_id` | `VARCHAR(64)` | Indexed, Nullable | Associated platform customer account |
| `stripe_payment_intent_id` | `VARCHAR(255)`| Indexed, Unique, Not Null | Stripe `pi_...` identifier |
| `amount` | `INTEGER` | Not Null | Amount in smallest currency unit (e.g., cents) |
| `currency` | `VARCHAR(3)` | Not Null, Default: `'usd'` | ISO 4217 three-letter currency code |
| `status` | `VARCHAR(32)` | Not Null, Default: `'REQUIRES_PAYMENT_METHOD'` | Payment state (see enum below) |
| `client_secret` | `VARCHAR(255)`| Not Null | Stripe client secret for frontend Elements mount |
| `created_at` | `TIMESTAMP` | Not Null, Default: `NOW()` | Record creation timestamp |
| `updated_at` | `TIMESTAMP` | Not Null, Default: `NOW()` | Last state update timestamp |

#### 2. Refund (`payment_refunds`)
Records full or partial refund operations executed against a successful payment.

| Column | Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | `UUID` | Primary Key, Default: `gen_random_uuid()` | Internal refund record ID |
| `payment_id` | `UUID` | Foreign Key (`payments.id`), Not Null | Associated internal payment record |
| `stripe_refund_id` | `VARCHAR(255)`| Indexed, Unique, Not Null | Stripe `re_...` identifier |
| `amount` | `INTEGER` | Not Null | Amount refunded in smallest currency unit |
| `currency` | `VARCHAR(3)` | Not Null | Currency of the refund |
| `reason` | `VARCHAR(64)` | Not Null, Default: `'requested_by_customer'` | Stripe reason code or internal justification |
| `status` | `VARCHAR(32)` | Not Null | `PENDING`, `SUCCEEDED`, `FAILED` |
| `created_by` | `VARCHAR(64)` | Not Null | User ID / Admin ID who triggered the refund |
| `created_at` | `TIMESTAMP` | Not Null, Default: `NOW()` | Record creation timestamp |

### State Machine (`PaymentStatus`)
```
[REQUIRES_PAYMENT_METHOD] 
           │
           ▼
[REQUIRES_CONFIRMATION / PROCESSING]
      │                     │
      ▼                     ▼
 [SUCCEEDED]             [FAILED]
      │
      ├────────────────────────┐
      ▼                        ▼
[PARTIALLY_REFUNDED]       [REFUNDED]
```

---

## Architecture & Logic Boundaries

The payment subsystem is structured using a CQRS-inspired Service Layer pattern within the TypeScript application.

```
┌────────────────────────────────────────────────────────┐
│                      Client App                        │
│             (Stripe Elements Web Frontend)             │
└───────────┬────────────────────────────────┬───────────┘
            │ 1. POST /create-intent         │ 3. confirmCardPayment(clientSecret)
            ▼                                ▼
┌──────────────────────────────┐    ┌────────────────────┐
│      Payment Controller      │    │    Stripe API      │
└───────────┬──────────────────┘    └────────┬───────────┘
            │ 2. Create Intent               │ 4. Webhook: payment_intent.succeeded
            ▼                                ▼
┌────────────────────────────────────────────────────────┐
│             Stripe Service & CQRS Handlers             │
│   - CreatePaymentIntentCommand                         │
│   - ProcessWebhookCommand                              │
│   - ProcessRefundCommand                               │
│   - GetPaymentStatusQuery                              │
└───────────────────────────┬────────────────────────────┘
                            │
                            ▼
               ┌────────────────────────┐
               │    Database Storage    │
               │  (Payments & Refunds)  │
               └────────────────────────┘
```

### CQRS Commands & Queries
- **`CreatePaymentIntentCommand`**:
  - Validates order state and amount.
  - Calls Stripe `stripe.paymentIntents.create({ amount, currency, metadata: { orderId } })`.
  - Persists initial `Payment` record with status `REQUIRES_PAYMENT_METHOD`.
  - Returns `clientSecret` and `paymentIntentId`.
- **`ProcessWebhookCommand`**:
  - Verifies Stripe cryptographic signature using the raw HTTP request body and `STRIPE_WEBHOOK_SECRET`.
  - Guarantees idempotency via database transactions checking whether the event ID or state transition has already been applied.
  - Handles `payment_intent.succeeded`: Sets status to `SUCCEEDED`, triggers order fulfillment event.
  - Handles `payment_intent.payment_failed`: Sets status to `FAILED`, triggers notification.
  - Handles `charge.refunded`: Updates aggregate refunded amounts and sets status to `PARTIALLY_REFUNDED` or `REFUNDED`.
- **`ProcessRefundCommand`**:
  - Validates requested refund amount does not exceed the remaining refundable balance (`amount - total_refunded`).
  - Calls Stripe `stripe.refunds.create({ payment_intent, amount, reason })`.
  - Records the entry in `payment_refunds` and updates parent `Payment` status.
- **`GetPaymentStatusQuery`**:
  - Fetches payment and refund history by `orderId` or internal `paymentId`.

---

## API Endpoints & Request/Response Contracts

### 1. Create Payment Intent
Initiates payment for an order and returns credentials for frontend Stripe Elements.

- **Endpoint:** `POST /api/payments/create-intent`
- **Authentication:** Customer Bearer Token / Session
- **Request Body:**
```json
{
  "orderId": "ord_1029384756",
  "amount": 4999,
  "currency": "usd"
}
```
- **Response (201 Created):**
```json
{
  "paymentId": "7a3536fa-7467-4bf6-b49b-73a7c6f01481",
  "paymentIntentId": "pi_3MtwBwLkdIwHu7ix28a3tqPa",
  "clientSecret": "pi_3MtwBwLkdIwHu7ix28a3tqPa_secret_YrKJ...",
  "amount": 4999,
  "currency": "usd",
  "status": "REQUIRES_PAYMENT_METHOD"
}
```
- **Errors:**
  - `400 Bad Request`: Invalid amount, unsupported currency, or order is not payable.
  - `404 Not Found`: `orderId` does not exist.
  - `409 Conflict`: Active successful payment already exists for this order.

---

### 2. Stripe Webhook Listener
Receives asynchronous lifecycle events from Stripe infrastructure.

- **Endpoint:** `POST /api/payments/webhook`
- **Headers:** `Stripe-Signature: t=1614000000,v1=5257a8...`
- **Request Body:** Raw buffer / string representation of Stripe Event payload.
- **Response (200 OK):**
```json
{
  "received": true
}
```
- **Errors:**
  - `400 Bad Request`: Invalid signature or payload parsing error.

---

### 3. Issue Refund
Processes a full or partial refund for a captured payment.

- **Endpoint:** `POST /api/payments/refund`
- **Authentication:** Admin / Operator Bearer Token (`roles: ["admin", "finance"]`)
- **Request Body:**
```json
{
  "paymentIntentId": "pi_3MtwBwLkdIwHu7ix28a3tqPa",
  "amount": 2500,
  "reason": "requested_by_customer"
}
```
*Note: If `amount` is omitted or null, the entire remaining balance is refunded.*
- **Response (200 OK):**
```json
{
  "refundId": "ref_98a76bc4-1234-4a21-9dfa-80acbc90e99f",
  "stripeRefundId": "re_3MtwBwLkdIwHu7ix28a3tqPa_0000000",
  "paymentIntentId": "pi_3MtwBwLkdIwHu7ix28a3tqPa",
  "amount": 2500,
  "currency": "usd",
  "status": "SUCCEEDED",
  "remainingBalance": 2499
}
```
- **Errors:**
  - `400 Bad Request`: Refund amount exceeds captured balance or invalid reason code.
  - `401 Unauthorized` / `403 Forbidden`: Caller lacks admin/finance permissions.
  - `404 Not Found`: Associated payment intent does not exist.
  - `422 Unprocessable Entity`: Payment is not in a refundable state (e.g., failed or already fully refunded).

---

## Acceptance Criteria (Given/When/Then scenarios)

### Scenario 1: Initializing Checkout Intent via Stripe Elements
- **Given** an authenticated customer with an unpaid order `ord_001` totaling `$35.00` USD,
- **When** the client submits `POST /api/payments/create-intent` with `{ "orderId": "ord_001", "amount": 3500, "currency": "usd" }`,
- **Then** a new Stripe `PaymentIntent` is created with amount `3500` and currency `'usd'`,
- **And** a local record in `payments` is persisted with status `'REQUIRES_PAYMENT_METHOD'`,
- **And** the endpoint responds with HTTP `201` containing the `clientSecret` and `paymentIntentId`.

### Scenario 2: Successful Payment Webhook Processing
- **Given** a pending payment for order `ord_001` with `stripe_payment_intent_id` = `'pi_test_123'`,
- **When** Stripe dispatches a webhook event `payment_intent.succeeded` with a valid `Stripe-Signature` header,
- **Then** the webhook handler verifies the cryptographic signature using the configured signing secret,
- **And** the payment status in the database transitions to `'SUCCEEDED'`,
- **And** an internal domain event `OrderPaymentCompletedEvent` is emitted to fulfill the order,
- **And** the endpoint returns HTTP `200 { "received": true }`.

### Scenario 3: Duplicate Webhook Event Delivery (Idempotency)
- **Given** a payment that has already transitioned to `'SUCCEEDED'`,
- **When** a redundant `payment_intent.succeeded` event for the same `paymentIntentId` is received,
- **Then** the service detects the current state is already `'SUCCEEDED'`,
- **And** no duplicate fulfillment events or database corruptions occur,
- **And** the endpoint returns HTTP `200 { "received": true }`.

### Scenario 4: Webhook Signature Verification Failure
- **Given** incoming traffic targeting `POST /api/payments/webhook`,
- **When** the `Stripe-Signature` header is missing, expired, or calculated with an invalid secret,
- **Then** the webhook handler immediately aborts execution without modifying any database records,
- **And** responds with HTTP `400 Bad Request` and message `"Webhook signature verification failed"`.

### Scenario 5: Full and Partial Refunds
- **Given** a payment with captured amount `5000` ($50.00) in status `'SUCCEEDED'`,
- **When** an authorized admin requests a partial refund via `POST /api/payments/refund` with `amount: 2000`,
- **Then** Stripe `refunds.create` is invoked with `amount: 2000`,
- **And** a `payment_refunds` record is created,
- **And** the payment status is updated to `'PARTIALLY_REFUNDED'`,
- **When** a subsequent refund request is submitted for the remaining `3000`,
- **Then** Stripe `refunds.create` succeeds,
- **And** the payment status is updated to `'REFUNDED'`.

### Scenario 6: Disallowed Over-Refund
- **Given** a payment with captured amount `5000` and previously refunded amount `4000`,
- **When** an admin submits `POST /api/payments/refund` with `amount: 1500`,
- **Then** the request is rejected with HTTP `400 Bad Request`,
- **And** the error details state `"Refund amount exceeds refundable balance of 1000"`,
- **And** no call is dispatched to Stripe.

---

## Quality Gate & Verification Boundaries

1. **PCI-DSS Scope Limitation:**
   - Raw credit card data (PAN, CVV, expiry) **must never** touch backend servers or backend logs.
   - Frontend must only collect card numbers via Stripe Elements iframe/SDK and exchange them for tokenized PaymentMethod / PaymentIntent states.
2. **Environment & Secrets Configuration:**
   - Required environment variables:
     - `STRIPE_SECRET_KEY`: `sk_test_...` or `sk_live_...`
     - `STRIPE_WEBHOOK_SECRET`: `whsec_...`
     - `STRIPE_PUBLISHABLE_KEY`: `pk_test_...` or `pk_live_...` (exposed to frontend)
3. **Automated Testing Requirements:**
   - **Unit Tests:** Mock Stripe SDK using `@stripe/stripe-node` mock utilities to verify calculations, currency normalization, and error handling.
   - **Integration Tests:** Use Stripe CLI fixtures or Stripe Mock to simulate end-to-end webhook receipt and signature generation.
   - **Idempotency Verification:** Concurrency test sending identical webhook payloads simultaneously to ensure strict atomic database locks prevent double fulfillment.