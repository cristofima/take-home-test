# Docker Compose Setup

Run the entire application stack with Docker Compose (no local .NET SDK or Node.js required):

## Prerequisites
- Docker Desktop installed and running

## Quick Start

1. **Start all services:**
   ```bash
   docker-compose up --build
   ```

2. **Access the application:**
   - Frontend: http://localhost:4200
   - Backend API: http://localhost:56808/api/loans
   - SQL Server: localhost:1433

3. **Stop services:**
   ```bash
   docker-compose down
   ```

## Database Initialization

The backend automatically applies migrations on startup. Seed data is included.

**SQL Server Credentials:**
- Server: `localhost,1433`
- Username: `sa`
- Password: `YourStrong@Password123`

## Development

Rebuild after code changes:
```bash
docker-compose up --build
```

Clean volumes (reset database):
```bash
docker-compose down -v
```
