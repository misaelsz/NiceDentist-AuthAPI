# NiceDentist Authentication API

Simple auth API using ASP.NET Core, Clean Architecture, ADO.NET (no EF/Dapper/MediatR), and JWT.

## Projects
- src/NiceDentist.Auth.Domain – Entities
- src/NiceDentist.Auth.Application – Contracts and services
- src/NiceDentist.Auth.Infrastructure – ADO.NET repository
- src/NiceDentist.Auth.Api – Web API (JWT, Swagger)
- tests/NiceDentist.Auth.Tests – Unit tests

## Setup
1. Run DB script in `db-scripts/create_login_database.sql` to create DB and Users table.
2. Update `src/NiceDentist.Auth.Api/appsettings.json` connection string if needed.
3. Build and run the API.

## Endpoints
- POST /api/auth/register
- POST /api/auth/login
- GET /api/auth/public (anonymous)
- GET /api/auth/protected (requires JWT)
- GET /api/auth/admin (requires role Admin)

## Seed
Add a user via `POST /api/auth/register` and login to get a token.

## Docker
- Prerequisite: Docker Desktop.
- Start SQL Server + API:

```powershell
docker compose up --build
```

- API will be at http://localhost:8080. Swagger at /swagger.
- SQL Server exposed on 1433 (user: sa, password: `Your_strong_password123!`). Change these for production.
