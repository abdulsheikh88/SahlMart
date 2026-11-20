# SHALMART E-commerce Platform

SHALMART is a fast, scalable e-commerce platform that combines the wide catalog, search, offers and order management of a large online marketplace with the hyperlocal ordering and rapid delivery of a quick-commerce service. Customers order from a nearby dark store and receive deliveries in minutes. Staff run the business from a separate, secured admin portal.

**Owner:** ABDULKADIR SHEIKH

> Status: scaffold (v0.1.0). Business logic, database migrations and integrations are marked with `TODO` and follow the Technology and Functional Document.

---

## Table of contents
1. [Product overview](#1-product-overview)
2. [Applications](#2-applications)
3. [Technology stack](#3-technology-stack)
4. [Architecture](#4-architecture)
5. [Authentication and authorization](#5-authentication-and-authorization)
6. [Repository structure](#6-repository-structure)
7. [Getting started](#7-getting-started)
8. [Configuration](#8-configuration)
9. [API overview](#9-api-overview)
10. [Security and compliance](#10-security-and-compliance)
11. [Branching, CI and releases](#11-branching-ci-and-releases)
12. [Roadmap](#12-roadmap)
13. [Contributing](#13-contributing)
14. [Contacts and licence](#14-contacts-and-licence)

---

## 1. Product overview

**Key capabilities**
- Location and pincode serviceability, mapped to a dark store
- Store-scoped catalog, pricing and stock (only what the nearest store can deliver)
- Fast typo-tolerant search
- Cart, coupons, checkout with UPI, cards, wallets and Cash on Delivery
- Order lifecycle with live status and rider tracking
- Admin portal for catalog, stores, inventory, orders, offers, users and reports
- Dark store picker app and rider app (Phase 2)

**Order lifecycle:** Placed, Payment Confirmed, Picking, Packed, Rider Assigned, Out for Delivery, Delivered. Alternate paths: Cancelled, Payment Failed, Partially Fulfilled, Refunded.

## 2. Applications

| App | Users | Location | Tech |
|---|---|---|---|
| Customer web (PWA) | Shoppers | `apps/customer-web` | Next.js, TypeScript, Tailwind |
| Mobile app | Shoppers | `apps/mobile` | React Native (Expo) |
| Admin portal | Staff | `apps/admin-portal` | React, Vite, TypeScript, MSAL |
| Store and rider apps | Pickers, riders | `apps/` (Phase 2) | React Native |
| API | All apps | `backend/` | ASP.NET Core (.NET 10), modular monolith |

## 3. Technology stack

| Layer | Technology |
|---|---|
| Front end | Next.js, React, TypeScript, Tailwind CSS, React Query, Zustand |
| Admin | React, Vite, TypeScript, Microsoft Authentication Library (MSAL) |
| Back end | .NET 10 LTS, ASP.NET Core minimal APIs, EF Core and Dapper, CQRS with MediatR (to be added) |
| Database | PostgreSQL with PostGIS |
| Cache | Redis |
| Search | Azure AI Search (alternatives: Typesense, Elasticsearch) |
| Messaging | Azure Service Bus |
| Real time | Azure SignalR or Web PubSub |
| Storage and CDN | Azure Blob Storage, Azure Front Door |
| Identity | Custom OTP service or Entra External ID (customers), Microsoft Entra ID (staff) |
| Hosting | Azure Container Apps, later AKS; region Central India with South India for DR |
| Observability | Application Insights, Azure Monitor, Grafana |
| CI/CD and IaC | GitHub Actions, Bicep or Terraform |

## 4. Architecture

```
Customer Web / Mobile     Admin Portal     Store and Rider apps
          \                    |                  /
           Azure Front Door + WAF + CDN
                         |
                  API (ASP.NET Core)
        /api/v1/customer/*   /api/v1/admin/*
                         |
   Modules: Identity | Catalog | Inventory | Orders | Payments
            (later) Pricing | Dispatch | Notifications | Stores | Reporting | Audit
                         |
        PostgreSQL | Redis | AI Search | Service Bus
                         |
        Payment gateway | SMS | Maps | Push | Email
```

**Key design decisions** (see `docs/adr/`)
1. Modular monolith first; extract Inventory, Orders, Dispatch and Search when load requires it.
2. Every price and stock level is keyed by `store_id`; the customer's location resolves to one store.
3. Inventory reads come from Redis; stock is reserved at checkout with a TTL and decremented in a database transaction.
4. Idempotent checkout using an `Idempotency-Key` header; payment gateway webhooks are the source of truth for payment status.
5. Order state changes publish events that drive inventory, notifications, dispatch and reporting.

## 5. Authentication and authorization

Customers and staff are separate identity populations. Nothing is shared between them.

| | Customer | Admin and staff |
|---|---|---|
| Login | Mobile OTP | Email and password with mandatory MFA (Entra ID) |
| Host | `www.<domain>` and mobile apps | `admin.<domain>` |
| API surface | `/api/v1/customer/*` | `/api/v1/admin/*` |
| Token | Access 15 min, rotating refresh 30 days | Access 10 min, 8 hour session, 30 min idle |
| Registration | Self sign-up | Invite only |
| Authorization | Ownership based | RBAC with permission claims and store scoping |

**Admin roles:** Super Admin, Operations Manager, Catalog Manager, Marketing Manager, Store Manager (own store only), Picker/Packer, Support Agent, Finance.

Permissions are claims such as `catalog.edit`, `inventory.write`, `orders.read` and `orders.refund`. Store-scoped roles also carry a `store_ids` claim that handlers must verify. High-risk actions (large refunds, bulk price edits, role changes) require re-authentication or a second approver.

## 6. Repository structure

```
shalmart-platform/
├── apps/
│   ├── customer-web/        Next.js storefront
│   ├── admin-portal/        React admin portal
│   └── mobile/              React Native app (placeholder)
├── backend/
│   ├── Shalmart.sln
│   ├── src/
│   │   ├── Shalmart.Api/                 Host, auth, CORS, rate limiting, route groups
│   │   ├── Shalmart.SharedKernel/        IModule, policies, shared types
│   │   └── Shalmart.Modules.*/           Identity, Catalog, Inventory, Orders, Payments
│   └── tests/Shalmart.Api.Tests/
├── docker/initdb/           PostgreSQL extensions (PostGIS)
├── docs/                    Technical docs and ADRs
├── infra/                   Infrastructure as code (to be added)
├── scripts/setup.sh
├── .github/                 CI, CodeQL, CODEOWNERS, PR template
├── docker-compose.yml       Local PostgreSQL and Redis
└── .env.example
```

Each backend module implements `IModule`: it registers its services and maps its own customer and admin endpoints. To add a module, create `Shalmart.Modules.<Name>`, implement `IModule`, reference it from `Shalmart.Api` and add it to the `modules` array in `Program.cs`.

## 7. Getting started

**Prerequisites:** .NET 10 SDK, Node.js 22 LTS, Docker Desktop, Git.

```bash
git clone https://github.com/<your-org>/shalmart-platform.git
cd shalmart-platform

./scripts/setup.sh              # creates .env from .env.example
# edit .env and set local passwords
docker compose up -d            # PostgreSQL (PostGIS) and Redis

# API
cd backend
dotnet restore
dotnet run --project src/Shalmart.Api      # health check at /health

# Customer web (new terminal)
cd apps/customer-web && npm install && npm run dev      # http://localhost:3000

# Admin portal (new terminal)
cd apps/admin-portal && npm install && npm run dev      # http://localhost:5173
```

**Tests**
```bash
cd backend && dotnet test
```

## 8. Configuration

Copy `.env.example` to `.env`. In .NET, use `__` for nested keys (for example `Auth__Admin__Authority`). Never commit real secrets. In Azure, store secrets in Key Vault and access them through managed identities.

| Setting | Purpose |
|---|---|
| `ConnectionStrings__Postgres` | PostgreSQL connection |
| `ConnectionStrings__Redis` | Redis connection |
| `Auth__Customer__Authority` / `Audience` | Customer token issuer and audience |
| `Auth__Admin__Authority` / `Audience` | Microsoft Entra ID tenant and API audience |
| `Cors__CustomerOrigins`, `Cors__AdminOrigins` | Allowed origins per surface |
| `NEXT_PUBLIC_API_BASE_URL` | Customer web API base URL |
| `VITE_API_BASE_URL` | Admin portal API base URL |

## 9. API overview

Base paths: `/api/v1/customer` and `/api/v1/admin`. A customer token is never accepted on an admin endpoint.

| Method | Path | Notes |
|---|---|---|
| GET | `/health` | Public health check |
| POST | `/api/v1/customer/auth/otp/request` | Send OTP (rate limited) |
| POST | `/api/v1/customer/auth/otp/verify` | Verify OTP, issue tokens |
| GET | `/api/v1/customer/catalog/products?storeId=` | Store-scoped catalog |
| POST | `/api/v1/customer/orders` | Idempotent checkout |
| GET | `/api/v1/customer/orders` | Own orders |
| POST | `/api/v1/customer/payments/intent` | Create gateway order |
| GET | `/api/v1/admin/catalog/products` | Requires `catalog.edit` |
| GET | `/api/v1/admin/inventory/{storeId}` | Requires `inventory.write` and store scope |
| GET | `/api/v1/admin/orders` | Requires `orders.read` |
| POST | `/api/v1/admin/payments/{id}/refund` | Requires `orders.refund` |

Endpoints that return `501 Not Implemented` are placeholders to be built. In Development, the OpenAPI document is served at `/openapi/v1.json`.

## 10. Security and compliance

- OWASP Top 10 and ASVS based development; STRIDE threat model before each major feature.
- WAF, DDoS protection and rate limiting; OTP abuse and SMS pumping protection.
- Refresh token rotation with reuse detection; server-side validation of prices, stock and discounts.
- Strict CORS per surface, security headers, private networking for databases.
- Admin: mandatory MFA, conditional access, invite-only accounts, immutable audit log.
- **PCI DSS:** card data is handled only by the hosted payment gateway; PAN and CVV are never stored.
- **DPDP Act (India):** consent, data minimisation, retention policy, and export and delete workflows.
- **GST:** compliant invoices, HSN codes and tax slabs.
- CodeQL, Dependabot and secret scanning are enabled through `.github/workflows`; a penetration test is required before go-live.

Report vulnerabilities privately to the security contact below. Do not open public issues for them.

## 11. Branching, CI and releases

- `main`: production. `develop`: integration. `feature/*`, `fix/*`: short-lived branches.
- Pull requests need one review (two for `infra/` and auth changes) and green CI.
- CI (`.github/workflows/ci.yml`) builds and tests the API, then type-checks and builds both web apps. CodeQL runs weekly and on pull requests.
- Commit style: Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:`).
- Environments: Dev, QA, UAT, Production. Production deploys need manual approval.

## 12. Roadmap

| Phase | Scope |
|---|---|
| 0. Discovery (2 weeks) | Workshops, UX direction, threat model, backlog, cloud setup |
| 1. MVP (8 to 12 weeks) | Serviceability, catalog, search, cart, checkout and payments, OTP login, order status, admin core |
| 2. Operations (6 to 8 weeks) | Picker app, rider app, live tracking, advanced offers, campaigns, reports, refunds workflow |
| 3. Growth | Recommendations, batching, dynamic pricing, loyalty, scheduled delivery, marketplace features, service extraction |

## 13. Contributing

1. Create a branch from `develop`.
2. Keep customer and admin code paths separate; add tests for new endpoints.
3. Run `dotnet test` and `npm run typecheck` before pushing.
4. Open a pull request using the template and request review from CODEOWNERS.

## 14. Contacts and licence

- Owner and product owner: ABDULKADIR SHEIKH
- Tech lead: _to be added_
- Security contact: _to be added_

Copyright (c) SHALMART. Owner: ABDULKADIR SHEIKH. All rights reserved. Proprietary and confidential; not licensed for redistribution.
