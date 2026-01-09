# Fundo Loan Management System

A full-stack Loan Management System built with **.NET 10** and **Angular 19**, following Clean Architecture principles with a focus on maintainability and best practices.

---

## Project Architecture

### Backend - .NET 10 Web API
- **Clean Architecture** with clear layer separation:
  - **Domain**: Core entities with business logic (`Loan` entity with validation methods)
  - **Application**: Business services, DTOs, and interfaces
  - **Infrastructure**: Data access with EF Core, repositories, and migrations
  - **WebApi**: RESTful API controllers and configuration
- **Entity Framework Core 10** with SQL Server
- **Dependency Injection** properly configured across layers
- **SLNX solution format** (new .NET 10 XML-based solution files)

### Frontend - Angular 19
- **Standalone components** (latest Angular architecture)
- **Angular Material** for UI components
- Consumes backend REST API

---

## API Endpoints

All endpoints are prefixed with `/api/loans`:

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/loans` | List all loans |
| `GET` | `/api/loans/{id}` | Get loan by ID |
| `POST` | `/api/loans` | Create new loan |
| `POST` | `/api/loans/{id}/payment` | Process payment (deduct from balance) |

**Loan Model:**
```json
{
  "id": "guid",
  "amount": 1500.00,
  "currentBalance": 500.00,
  "applicantName": "Maria Silva",
  "status": "active",
  "createdAt": "2026-01-07T10:00:00Z",
  "updatedAt": "2026-01-07T10:00:00Z"
}
```

---

## Technology Stack

**Backend:**
- .NET 10 SDK
- Entity Framework Core 10
- SQL Server (Windows Authentication)
- xUnit (testing framework)

**Frontend:**
- Angular 19.1
- TypeScript 5.7
- Angular Material 19.2

---

## Setup Instructions

### Prerequisites
- .NET 10 SDK
- SQL Server (LocalDB or full instance)
- Node.js 18+ and npm
- Visual Studio 2025 or VS Code

### Backend Setup

1. **Navigate to backend source:**
   ```powershell
   cd backend/src
   ```

2. **Restore packages:**
   ```powershell
   dotnet restore
   ```

3. **Apply database migrations:**
   ```powershell
   dotnet ef migrations add InitialCreate --project Fundo.Infrastructure --startup-project Fundo.WebApi
   ```

4. **Run the API:**
   ```powershell
   cd Fundo.WebApi
   dotnet run
   ```
   API runs at: `https://localhost:56807` and `http://localhost:56808`

   > **Note**: For local development, the frontend uses HTTP (`http://localhost:56808`) to avoid SSL certificate issues. If you prefer HTTPS, run: `dotnet dev-certs https --trust`

5. **Run tests:**
   ```powershell
   cd ..
   dotnet test
   ```

### Frontend Setup

1. **Navigate to frontend:**
   ```powershell
   cd frontend
   ```

2. **Install dependencies:**
   ```powershell
   npm install
   ```

3. **Start development server:**
   ```powershell
   npm start
   ```
   App runs at: `http://localhost:4200`

---

## Docker Setup ✅ (Recommended)

Run the entire stack without installing .NET SDK or Node.js (from project root):

```bash
docker-compose up --build
```

- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:56808/api/loans
- **SQL Server 2019**: Automatic migrations on startup with 5 seeded loans

See [DOCKER.md](DOCKER.md) for details.

**Image Sizes:**
- SQL Server 2019: 1.49GB
- Backend (.NET 10): ~450MB
- Frontend (nginx): ~50MB

---

## CI/CD with GitHub Actions ✅

Automated build and test pipeline configured in [.github/workflows/backend.yml](.github/workflows/backend.yml):

**Triggers:**
- Push to `main` branch
- Pull requests targeting that branch
- Manual workflow dispatch

**Pipeline Steps:**
1. Checkout repository
2. Setup .NET 10 SDK
3. Restore dependencies
4. Build solution in Release mode
5. Run Unit Tests (21 tests using Moq)
6. Run Integration Tests (22 tests using TestContainers with SQL Server)
7. Upload test results as artifacts
8. Publish test results report

**Key Features:**
- `ubuntu-latest` runner has Docker pre-installed for TestContainers
- Separate test runs for unit and integration tests
- TRX test result logging with artifact retention
- Test result reporting with `dorny/test-reporter`

---

## Implementation Highlights

### ✅ Completed Features
- All 4 REST endpoints implemented with Clean Architecture
- Service layer with business logic in Application layer (not Infrastructure)
- EF Core integration with SQL Server and migrations
- Database seeded with 5 sample loans
- Domain-Driven Design with `Loan.IsValid()` and `Loan.UpdateStatus()` methods
- **Type-safe constants**: `LoanStatus.Active` and `LoanStatus.Paid` eliminate magic strings
- Proper Dependency Injection in both Application and Infrastructure layers
- CORS configured for Angular frontend
- Project upgraded to latest .NET 10 with SLNX solution format
- **Structured Logging**: Serilog with console and file outputs, request logging
- **API Documentation**: Swagger/OpenAPI with detailed endpoint documentation

### ✅ DevOps & Testing
- **Docker**: docker-compose.yml with SQL Server 2019, backend, and frontend containers
- **Unit Tests**: 21 tests for domain entities and application services (Moq)
- **Integration Tests**: 22 tests with TestContainers (real SQL Server 2022)
- **GitHub Actions**: CI/CD pipeline triggered on C# file changes

### 🔧 Project Structure
```
backend/src/
├── Fundo.Domain/              # Entities with business logic
├── Fundo.Application/         # Services, DTOs, interfaces
├── Fundo.Infrastructure/      # EF Core, repositories, migrations
├── Fundo.WebApi/              # Controllers, API configuration
├── Fundo.Services.Tests/      # Unit and Integration tests
└── Fundo.slnx                 # .NET 10 XML solution file

frontend/src/app/
├── components/loans/          # Loan listing component
├── models/                    # TypeScript interfaces
├── services/                  # HTTP services
└── app.config.ts              # Standalone app configuration
```

---

## Development Challenges

### 1. Integration Testing with .NET 10
**Challenge**: SQLite in-memory database (Microsoft's recommended test fake) failed with "duplicate database provider" errors when used with `WebApplicationFactory` in .NET 10.

**Root Cause**: .NET 10's service provider lifecycle initializes the SQL Server DbContext before `ConfigureTestServices` executes, preventing clean provider swapping. Multiple attempts to remove/replace service descriptors all failed due to the timing of EF Core's service registration.

**Solution**: Adopted **TestContainers** with real SQL Server 2022 instances running in Docker. This provides:
- Production parity (same database engine as production)
- No provider conflicts (uses actual SQL Server connection string)
- Industry-standard approach for integration testing
- Support for all SQL Server features (GETUTCDATE(), stored procedures, etc.)

**Impact**: All 22 integration tests pass with real database transactions, providing higher confidence than in-memory fakes.

---

### 2. Framework Migration (.NET 6 → .NET 10)
**Challenge**: Project requirements specified .NET 6, but upgrading to .NET 10 enabled modern features and long-term support.

**Solution**: Systematic migration of all `.csproj` files to `net10.0`, upgraded EF Core to 10, and migrated to SLNX solution format.

**Trade-offs**:
- ✅ Latest features and performance improvements
- ✅ Better tooling support (TestContainers, improved minimal APIs)
- ⚠️ Required re-testing all functionality
- ⚠️ Some package compatibility issues resolved

---

### 3. EF Core Seed Data Determinism
**Challenge**: Using `DateTime.UtcNow` in seed data caused "model changes detected" warnings on every build, triggering unnecessary migration prompts.

**Root Cause**: EF Core compares model snapshots; dynamic values in `OnModelCreating()` generate different snapshots each time.

**Solution**: Replaced all `DateTime.UtcNow` calls with static timestamps in seed data:
```csharp
// ❌ Before (non-deterministic)
CreatedAt = DateTimeOffset.UtcNow

// ✅ After (deterministic)
CreatedAt = new DateTimeOffset(2025, 7, 1, 0, 0, 0, TimeSpan.Zero)
```

**Lesson**: Never use dynamic values in EF Core model configuration or seed data.

---

### 4. Clean Architecture Enforcement
**Challenge**: Initial project structure had service logic leaking into Infrastructure layer, violating Clean Architecture principles.

**Solution**: Restructured to enforce strict layer boundaries:
- Moved business services from Infrastructure to Application layer
- Kept repositories in Infrastructure (data access only)
- Domain entities contain business logic methods (e.g., `Loan.UpdateStatus()`)
- Infrastructure only implements interfaces defined in Application

**Result**: Clear separation of concerns with unidirectional dependencies (Domain ← Application ← Infrastructure ← WebApi).

---

### 5. .NET 10 Build Artifacts
**Challenge**: `BuildHost-net472` and `BuildHost-netcore` folders appearing in Visual Studio Solution Explorer after installing EF Core Design tools.

**Root Cause**: .NET 10 changed NuGet content file handling; Roslyn workspace artifacts from `Microsoft.CodeAnalysis.Workspaces.MSBuild` (transitive dependency) became visible by default.

**Solution**: Added to `.csproj` files:
```xml
<PropertyGroup>
  <DefaultItemExcludes>$(DefaultItemExcludes);**/BuildHost-*/**</DefaultItemExcludes>
</PropertyGroup>
```

---

## Architectural Decisions

### 1. Clean Architecture with Four Layers
**Decision**: Implement full Clean Architecture rather than simplified N-tier.

**Rationale**:
- Matches Fundo's production codebase structure
- Enforces testability through dependency inversion
- Domain remains technology-agnostic (zero dependencies)
- Easy to swap infrastructure implementations (e.g., SQL Server → PostgreSQL)

**Trade-off**: More initial setup complexity vs. long-term maintainability and flexibility.

---

### 2. Service Layer in Application, Not Infrastructure
**Decision**: Business logic services (`LoanService`) belong in Application layer, not Infrastructure.

**Rationale**:
- Infrastructure should only handle data access (repositories)
- Application layer orchestrates business operations
- Keeps business rules out of data access code
- Follows Domain-Driven Design principles

**Pattern**:
```
Application/Services/LoanService.cs    ← Business logic
Infrastructure/Repositories/LoanRepository.cs  ← Data access only
```

---

### 3. Domain Entities with Business Logic
**Decision**: Domain entities contain validation and business rule methods, not just data properties.

**Example**:
```csharp
public class Loan
{
    public bool IsValid() => CurrentBalance <= Amount && CurrentBalance >= 0;
    
    public void UpdateStatus()
    {
        if (CurrentBalance <= 0) Status = "paid";
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
```

**Rationale**:
- Rich domain model vs. anemic model
- Business rules centralized in domain
- Prevents invalid state transitions
- Self-documenting business logic

---

### 4. TestContainers Over In-Memory Databases
**Decision**: Use real SQL Server in Docker containers for integration tests instead of SQLite/in-memory providers.

**Rationale**:
- **Production parity**: Tests run against same database engine as production
- **Feature support**: SQL Server-specific features (GETUTCDATE(), transactions, indexes) work correctly
- **.NET 10 compatibility**: Avoids provider conflict issues
- **Confidence**: Higher trust in test results

**Trade-off**: Tests take 30-40 seconds (container startup) vs. 5 seconds with in-memory, but only once per test class.

---

### 5. Windows Authentication for Local Development
**Decision**: Use Windows Authentication (`Integrated Security=True`) instead of SQL Server authentication.

**Rationale**:
- No password management in connection strings
- Better security (uses Windows credentials)
- Simpler local development setup
- Matches typical enterprise SQL Server configurations

**Production Note**: Use Azure AD authentication or managed identities when deploying to cloud.

---

### 6. Plural API Routes
**Decision**: Use `/api/loans` (plural) for resource collections, not `/api/loan` (singular).

**Rationale**:
- RESTful convention: collections are plural
- Matches Microsoft API design guidelines
- More intuitive for developers consuming the API

**Example**:
```
GET    /api/loans       ← List all loans
POST   /api/loans       ← Create new loan
GET    /api/loans/{id}  ← Get specific loan
```

---

### 7. SLNX Solution Format (.NET 10)
**Decision**: Migrated from legacy `.sln` to new `.slnx` XML-based solution format.

**Rationale**:
- Simpler, human-readable XML structure
- Better for version control (fewer merge conflicts)
- Native to .NET 10 tooling
- Future-proof for .NET evolution

**Migration**: `dotnet new sln --format slnx` + re-add all projects

---

## Future Enhancements

Potential improvements for production readiness:

- **Pagination**: Implement page-based or cursor-based pagination for `/api/loans` endpoint to handle large datasets efficiently
- **E2E Frontend Tests**: Implement end-to-end testing using Playwright for automated UI testing
- **Authentication & Authorization**: JWT tokens, role-based access control, Azure AD integration
- **Rate Limiting**: Protect API endpoints from abuse
- **Caching**: Redis/in-memory caching for frequently accessed data
- **API Versioning**: Support multiple API versions for backward compatibility
- **Monitoring**: Application Insights, health checks, metrics dashboards

---

## Notes

- **Architecture**: Clean Architecture strictly enforced - Domain has zero dependencies, only Infrastructure references EF Core
- **API Routes**: Controllers use plural routes (`/api/loans`), not singular
- **Database**: Migrations managed from Infrastructure project, connection string in WebApi's `appsettings.json`
- **Testing**: Integration test project references all layers; uses TestContainers for isolation
