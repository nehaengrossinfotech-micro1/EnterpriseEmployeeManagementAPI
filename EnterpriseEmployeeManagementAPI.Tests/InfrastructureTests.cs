using EnterpriseEmployeeManagementAPI.Data;
using EnterpriseEmployeeManagementAPI.HealthChecks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EnterpriseEmployeeManagementAPI.Tests;

public sealed class InfrastructureTests
{
    [Fact]
    public async Task SeedDataCreatesExpectedRecordsAndIsIdempotent()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using var services = new ServiceCollection()
            .AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(databaseName))
            .BuildServiceProvider();

        await SeedData.InitializeAsync(services);
        await SeedData.InitializeAsync(services);

        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await context.Departments.CountAsync()).Should().Be(2);
        (await context.Employees.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task DatabaseHealthCheckReportsHealthyForReachableDatabase()
    {
        await using var services = new ServiceCollection()
            .AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .BuildServiceProvider();
        var healthCheck = new DatabaseHealthCheck(
            services.GetRequiredService<IServiceScopeFactory>());

        var result = await healthCheck.CheckHealthAsync(
            new HealthCheckContext(),
            CancellationToken.None);

        result.Status.Should().Be(HealthStatus.Healthy);
    }
}
