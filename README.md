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
2. Start the database stack from the `Database` folder (SQL Server + schema init).
3. Start the API from the repo root.
4. Open Swagger and test.

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

### 1) Database (from `Database/`)
```powershell
cd Database
docker compose --env-file db.env up -d --build
docker compose logs -f sqlserver
docker compose logs -f db-init
```

By default it exposes SQL on `${SQL_PORT:-1433}`. DB name is `${DB_NAME:-NiceDentistAuthDb}`.
Credentials: `sa` / value in `db.env` (`SA_PASSWORD`).

### 2) API (from repo root)
```powershell
cd ..
docker compose up -d --build
docker compose logs -f authapi
```

Swagger: http://localhost:${API_PORT:-8080}/swagger

### Stop and clean
- Database (from `Database/`):
```powershell
cd Database
docker compose down
docker compose down -v   # also removes DB volume
```

- API (from repo root):
```powershell
docker compose down
```

### Troubleshooting
- If port 1433 is in use, set `SQL_PORT` in `Database/db.env` (e.g. 1434) and re-run DB stack.
- If API can’t connect to DB, ensure DB is running (from Database compose) and `DB_HOST/SQL_PORT` in root `.env` point to it.
- To reset DB, remove volume from the Database stack: `docker compose down -v` in the `Database` folder.
