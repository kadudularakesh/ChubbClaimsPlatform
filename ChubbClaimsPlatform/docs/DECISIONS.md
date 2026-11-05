# Architecture and Design Decisions

## 1. Shape
A modular monolith with one ASP.NET Core API and one Angular SPA. The assessment does not require microservices; a single deployable is faster to build, debug and run locally. Domain boundaries are kept explicit so Claims, Identity, Workflow and Reporting can be separated later if scale or team ownership justifies it.

## 2. Synchronous vs asynchronous
REST is used for user-driven commands and queries where the caller needs an immediate result: login, claim submission, assignment, information response, approval, rejection and settlement.

Kafka is a sensible future transport for non-blocking side effects: notification delivery, downstream analytics and integration events. The MVP does not require Kafka to run. A production implementation should write domain events to an outbox in the same database transaction, then publish them to Kafka with idempotent consumers. This avoids the dual-write problem.

## 3. Claim workflow
The API owns state transitions. The Angular client cannot set arbitrary status values. Every transition writes a ClaimStatusHistory record. Rejection requires a reason; approval requires an approved amount; settlement requires a settlement amount and only follows approval.

## 4. Liability
The demo dashboard calculates operational outstanding exposure as the sum of approved amount where present, otherwise estimated amount, for claims not Rejected or Settled. This is an illustrative management metric, not a formal actuarial reserve. Real rules must define incurred-but-not-reported claims, recoveries, currencies, FX rates, deductibles and reserve adjustments.

## 5. Data ownership and security
Claimants can access only their own claims. Claims officers can access the operational queue. Internal notes are not returned to claimant-facing responses. JWT role claims are checked by API authorization policies.

## 6. Contract
The API DTOs and Swagger/OpenAPI document define the backend contract. In a larger team, generate a typed Angular client from OpenAPI to reduce contract drift.

## 7. Multi-market
The Claim includes a market code and currency code. A production solution must define market-specific policy validation, data residency, currency conversion and regulatory retention rules.

## 8. Production hardening
Add identity provider integration, refresh-token rotation, MFA, rate limiting, robust audit, secure object storage and malware scanning, optimistic concurrency, pagination, structured telemetry, outbox/Kafka, integration tests, CI/CD and secrets management.
