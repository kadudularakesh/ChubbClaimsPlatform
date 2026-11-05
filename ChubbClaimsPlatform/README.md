# Chubb APAC Claims Management Platform

A runnable MVP scaffold for the Chubb APAC take-home assessment. It uses one Angular application for Claimants and Claims Officers, backed by an ASP.NET Core 8 REST API and EF Core.

## Implemented MVP scope
- JWT login with `Claimant` and `ClaimsOfficer` roles
- Claimant: submit a Motor/Property claim, view own claims, view status/history, provide requested information
- Claims Officer: view queue, assign claim to self, request information, approve/reject, settle
- Dashboard counts and operational outstanding exposure
- Claim status history and internal notes
- Swagger API documentation
- SQLite by default for a frictionless local demo; SQL Server can be configured
- AI working journal template

The assessment intentionally asks candidates to make scope and architecture decisions. This is an MVP, not a production insurance platform. Read `docs/DECISIONS.md` before the walkthrough.

## Prerequisites
- .NET 8 SDK
- Node.js 20+ and npm
- Angular CLI (optional; use `npx ng`)

## Run backend
```bash
cd backend/ChubbClaims.Api
dotnet restore
dotnet run
```
Swagger: `http://localhost:5080/swagger` (or the URL printed by `dotnet run`).

The app creates the SQLite database and demo users on startup.

Demo accounts:
- Claimant: `claimant@demo.com` / `Passw0rd!`
- Claims Officer: `officer@demo.com` / `Passw0rd!`

## Run frontend
In a second terminal:
```bash
cd frontend
npm install
npm start
```
Open `http://localhost:4200`. The Angular dev server proxies `/api` to `http://localhost:5080`.

If your backend prints a different port, update `frontend/proxy.conf.json`.

## SQL Server option
Set the environment variable `ConnectionStrings__DefaultConnection` to a SQL Server connection string and set `Database__Provider=SqlServer`. Ensure the SQL Server EF provider package is restored. The default local path uses SQLite so the project can be run without a SQL Server installation.

## Main API routes
- `POST /api/auth/login`
- `GET /api/claims/mine`
- `POST /api/claims`
- `GET /api/claims`
- `GET /api/claims/{id}`
- `POST /api/claims/{id}/assign`
- `POST /api/claims/{id}/request-information`
- `POST /api/claims/{id}/provide-information`
- `POST /api/claims/{id}/approve`
- `POST /api/claims/{id}/reject`
- `POST /api/claims/{id}/settle`
- `GET /api/dashboard/officer`

## Important limitations / next steps
- Demo authentication is intentionally simple; add refresh tokens, lockout, password reset and identity provider integration before production.
- Document upload is represented in the domain but file storage is not implemented. Add malware scanning and private object storage (e.g. Azure Blob) before enabling uploads.
- Kafka is documented as an integration boundary, not required to run the MVP. For reliable events, implement a transactional outbox and publish claim events to Kafka.
- Market, currency, policy coverage, fraud checks, reserving and formal actuarial liability rules need business confirmation.
- Add pagination, rate limiting, audit retention, integration tests, CI/CD and observability before production.

## Suggested demo flow
1. Sign in as claimant and create a claim.
2. Sign out and sign in as officer.
3. Assign the claim to yourself.
4. Request more information.
5. Sign in as claimant and provide information.
6. Sign in as officer and approve, then settle.
7. Show the officer dashboard and claim timeline.
