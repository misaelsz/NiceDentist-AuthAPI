# NiceDentist Authentication API

Simple auth API using ASP.NET Core, Clean Architecture, ADO.NET (no EF/Dapper/MediatR), and JWT.

## Projects
- src/NiceDentist.Auth.Domain – Entities
- src/NiceDentist.Auth.Application – Contracts and services
- src/NiceDentist.Auth.Infrastructure – ADO.NET repository
- src/NiceDentist.Auth.Api – Web API (JWT, Swagger)
- tests/NiceDentist.Auth.Tests – Unit tests

## Setup
1. Copy `.env.example` to `.env` in the repo root and adjust values if needed (never commit `.env`).
2. Prepare and run the database (see `Database/README.md`).
3. Start the API from this folder.
4. Open Swagger and test.

## Endpoints
- POST /api/auth/register
- POST /api/auth/login
- GET /api/auth/public (anonymous)
- GET /api/auth/protected (requires JWT)
- GET /api/auth/admin (requires role Admin)

## Seed
Add a user via `POST /api/auth/register` and login to get a token.

## Docker (API)
- Prerequisite: Docker Desktop. Ensure the database is running per `Database/README.md`.

```powershell
docker compose up -d --build
docker compose logs -f authapi
```

Swagger: http://localhost:${API_PORT:-8080}/swagger
