## Running the Backend

### Build
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
- All 22 integration tests use real SQL Server containers

### Run API
```powershell
cd Fundo.WebApi
dotnet run
```

API runs at: `https://localhost:5001` and `http://localhost:5000`

Test the API:
```http
GET https://localhost:5001/api/loans
```

---

## Testing Strategy

### Integration Tests
**Framework**: xUnit with `WebApplicationFactory<Program>`  
**Database**: TestContainers with SQL Server 2022  
**Location**: `Fundo.Services.Tests/Integration/`

**Coverage (22 tests)**:
- GET /api/loans - 5 tests (list, empty, sorting, content-type)
- GET /api/loans/{id} - 3 tests (success, not found, invalid format)
- POST /api/loans - 7 tests (success, validation scenarios)
- POST /api/loans/{id}/payment - 7 tests (payments, validation, lifecycle)

**Key Patterns Applied**:
- `IClassFixture<CustomWebApplicationFactory>` for shared test context
- `AllowAutoRedirect = false` to test actual status codes
- Descriptive test names: `MethodName_Scenario_ExpectedResult`
- Test both success and failure paths
- Validate status codes, headers, and response bodies
- Real SQL Server for production parity

**Running Specific Tests**:
```powershell
# Run only integration tests
dotnet test --filter FullyQualifiedName~LoanManagementControllerTests

# Run specific test
dotnet test --filter GetAllLoans_ReturnsOkStatusCode
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
- Business logic in Domain entities (`Loan.IsValid()`, `Loan.UpdateStatus()`)
- Repository interfaces in Application, implementations in Infrastructure
- DTOs for API contracts, separate from Domain entities
