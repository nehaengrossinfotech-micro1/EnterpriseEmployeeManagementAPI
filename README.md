# EnterpriseEmployeeManagementAPI

A production-oriented Employee Management REST API built with ASP.NET Core 10
and C#. The service uses SQL Server, Entity Framework Core, API versioning,
FluentValidation, structured Serilog output, health checks, JWT validation, and
a repository/service architecture.

## Project structure

```text
EnterpriseEmployeeManagementAPI/
├── .github/                              Repository automation and templates
├── EnterpriseEmployeeManagementAPI/     ASP.NET Core Web API
├── EnterpriseEmployeeManagementAPI.Tests/ xUnit unit tests
├── Directory.Build.props                Shared compiler and analyzer settings
└── EnterpriseEmployeeManagementAPI.sln  Solution
```

The API project separates HTTP endpoints, DTOs, domain entities, validation,
application services, repositories, EF Core data access, middleware, logging,
configuration, and health checks.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server, SQL Server Express, or LocalDB
- Git

## Setup

Clone the repository and restore dependencies:

```bash
git clone https://github.com/nehaengrossinfotech-micro1/EnterpriseEmployeeManagementAPI.git
cd EnterpriseEmployeeManagementAPI
dotnet restore EnterpriseEmployeeManagementAPI.sln
```

Build the complete solution:

```bash
dotnet build EnterpriseEmployeeManagementAPI.sln --configuration Release --no-restore
```

Run the API in development:

```bash
dotnet run --project EnterpriseEmployeeManagementAPI
```

On first startup the application creates the configured database when necessary
and seeds Engineering and People Operations departments plus two sample
employees.

## Database configuration

The default development connection uses SQL Server LocalDB:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EnterpriseEmployeeManagement;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

Override it without changing tracked configuration:

```powershell
$env:EEMA_ConnectionStrings__DefaultConnection = "Server=localhost;Database=EnterpriseEmployeeManagement;User Id=sa;Password=<password>;TrustServerCertificate=True"
```

For production, store the connection string and JWT secret in a managed secret
provider. Replace the development JWT secret with:

```powershell
$env:EEMA_Jwt__Secret = "<at-least-32-random-characters>"
```

The project includes EF Core design-time tooling. To adopt migrations instead of
startup database creation:

```bash
dotnet ef migrations add InitialCreate --project EnterpriseEmployeeManagementAPI
dotnet ef database update --project EnterpriseEmployeeManagementAPI
```

## API documentation

When `ASPNETCORE_ENVIRONMENT=Development`, Swagger UI is available at:

```text
https://localhost:7205/swagger
```

Version 1 endpoints:

| Method | Endpoint | Description |
| --- | --- | --- |
| `GET` | `/api/v1/employees` | Retrieve all employees |
| `GET` | `/api/v1/employees/{id}` | Retrieve one employee |
| `GET` | `/api/v1/employees/search?query=value` | Search employees |
| `POST` | `/api/v1/employees` | Create an employee |
| `PUT` | `/api/v1/employees/{id}` | Update an employee |
| `DELETE` | `/api/v1/employees/{id}` | Delete an employee |
| `GET` | `/health` | Check API and database health |

Validation failures return HTTP 400 problem details, missing resources return
404, duplicate email conflicts return 409, successful creation returns 201,
updates and deletes return 204, and unexpected failures return sanitized HTTP
500 problem details with a trace identifier.

## Testing and coverage

The test project uses xUnit, Moq, FluentAssertions, EF Core InMemory, and
Coverlet:

```bash
dotnet test EnterpriseEmployeeManagementAPI.sln \
  --configuration Release \
  --collect "XPlat Code Coverage" \
  --logger "trx;LogFileName=test-results.trx"
```

The generated TRX test report and Cobertura coverage file are written beneath
the test results directory.

## CI/CD and security

GitHub Actions run for every pull request and every push to `main`:

- `build.yml` restores, builds, publishes the API, and uploads the build output.
- `test.yml` runs all tests, collects coverage, and uploads TRX and Cobertura
  artifacts. Any failing test fails the job.
- `codeql.yml` runs extended CodeQL analysis, NuGet vulnerability auditing,
  dependency review, and a critical-severity security gate.

Dependabot checks NuGet and GitHub Actions dependencies weekly.

## Contributing

1. Create a focused branch from the latest `main`.
2. Keep controllers thin and place business behavior in services.
3. Add or update unit tests for every behavioral change.
4. Run restore, release build, and tests locally.
5. Open a pull request using the repository template.
6. Resolve required reviews from CODEOWNERS and all CI findings before merge.

Do not commit secrets, production connection strings, generated build output, or
test-result artifacts.

## License

This project is licensed under the [MIT License](LICENSE).
