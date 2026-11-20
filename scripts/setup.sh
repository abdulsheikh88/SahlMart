#!/usr/bin/env bash
# One-time local setup
set -euo pipefail
[ -f .env ] || cp .env.example .env
echo "Edit .env and set local passwords, then run: docker compose up -d"
echo "Backend:      cd backend && dotnet run --project src/Shalmart.Api"
echo "Customer web: cd apps/customer-web && npm install && npm run dev"
echo "Admin portal: cd apps/admin-portal && npm install && npm run dev"
