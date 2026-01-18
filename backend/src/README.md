## Running the Backend

### Build Locally
```powershell
cd backend/src
dotnet build
```

### Run Tests
```powershell
dotnet test --verbosity normal
```

**Prerequisites for tests:**
- Docker Desktop running (TestContainers requires Docker)
- All 22 integration tests use real SQL Server 2022 containers for production parity

### Run API
```powershell
cd Fundo.WebApi
dotnet run
```

API runs at: `https://localhost:56807` and `http://localhost:56808`

**Swagger UI**: Available at `https://localhost:56807/swagger` or `http://localhost:56808/swagger`

Test the API:
```http
GET https://localhost:56807/api/loans
```

**Logs**: Application logs are written to `logs/fundo-<date>.log` with rolling daily files

---

## Testing Strategy

**Test Summary**: 51 tests (22 integration + 29 unit)

### Integration Tests (22 tests)
**Framework**: xUnit with `WebApplicationFactory<Program>`  
**Database**: TestContainers with SQL Server 2022  
**Location**: `Fundo.Services.Tests/Integration/`

**Coverage**:
- GET /api/loans - 5 tests (list, empty, sorting, content-type)
- GET /api/loans/{id} - 3 tests (success, not found, invalid format)
- POST /api/loans - 7 tests (success, validation scenarios)
- POST /api/loans/{id}/payment - 7 tests (payments, validation, lifecycle)

**Key Patterns**:
- `IClassFixture<CustomWebApplicationFactory>` for shared test context
- Real SQL Server in Docker for production parity
- Descriptive test names: `MethodName_Scenario_ExpectedResult`
- Test both success and failure paths

### Unit Tests (29 tests)
**Framework**: xUnit with Moq for mocking  
**Focus**: Core business logic with zero external dependencies

#### Domain Tests (17 tests)
**Location**: `Fundo.Services.Tests/Unit/Domain/LoanTests.cs`

Tests `Loan` entity business logic:
- `Loan.Create()` factory method - 6 tests for validation
- `IsValid()` - 5 tests for balance validation rules
- `ApplyPayment()` - 6 tests for payment processing and validation

#### Application Tests (12 tests)
**Location**: `Fundo.Services.Tests/Unit/Application/LoanServiceTests.cs`

Tests `LoanService` with mocked `ILoanRepository`:
- `CreateLoanAsync` - 2 tests
- `GetLoanByIdAsync` - 2 tests
- `GetAllLoansAsync` - 2 tests
- `ProcessPaymentAsync` - 6 tests (payment processing, validation, status updates)

**Running Tests**:
```powershell
dotnet test                                          # All 51 tests
dotnet test --filter FullyQualifiedName~Unit        # 29 unit tests only
dotnet test --filter FullyQualifiedName~Integration # 22 integration tests
```

---

## Database Management

### Migrations
```powershell
# Create migration (run from Infrastructure directory)
cd Fundo.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../Fundo.WebApi

# Apply migrations
dotnet ef database update --startup-project ../Fundo.WebApi

# View SQL
dotnet ef migrations script --startup-project ../Fundo.WebApi
```

### Connection String
Located in `Fundo.WebApi/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FundoLoanDb;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

Uses Windows Authentication (`Integrated Security=True`)

---

## Architecture Notes

This backend follows **Clean Architecture** with strict layer separation:

**Dependency Flow**:
```
Fundo.WebApi (Controllers)
    ↓ references
Fundo.Application (Services, DTOs)
    ↓ references  
Fundo.Domain (Entities, Business Rules)
    ↑ references
Fundo.Infrastructure (EF Core, Repositories)
```

**Key Rules**:
- Domain has zero dependencies (pure C# entities)
- Application depends only on Domain
- Infrastructure references Domain + Application
- WebApi depends on Application (Infrastructure only for DI registration)

**Important Patterns**:
- Services in Application layer, NOT Infrastructure
- Business logic in Domain entities with encapsulation (`Loan.Create()`, `Loan.ApplyPayment()`)
- Type-safe constants for status values (`LoanStatus.Active`, `LoanStatus.Paid`)
- Repository interfaces in Application, implementations in Infrastructure
- DTOs for API contracts, separate from Domain entities
