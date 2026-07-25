# EnterpriseEmployeeManagementAPI

ASP.NET Core Web API foundation for an enterprise employee management service.

## Prerequisites

- .NET 10 SDK

## Run locally

```bash
dotnet restore
dotnet build --no-restore
dotnet run --project EnterpriseEmployeeManagementAPI
```

In development, Swagger UI is available at `/swagger` and the health endpoint is
available at `/health`.

## Configuration

Configuration is loaded from `appsettings.json`, environment-specific settings,
and environment variables prefixed with `EEMA_`.

## License

This project is licensed under the MIT License.
