# ADR 0002: Separate customer and admin identity

**Status:** Accepted

**Decision:** Customers sign in with mobile OTP and receive tokens from the customer issuer. Staff sign in through Microsoft Entra ID with MFA. The API exposes `/api/v1/customer/*` and `/api/v1/admin/*` as separate route groups, each with its own authentication scheme, audience, CORS policy and rate limits.

**Consequences:** A customer token can never be accepted on an admin endpoint. Staff offboarding and MFA are handled by Entra ID.
