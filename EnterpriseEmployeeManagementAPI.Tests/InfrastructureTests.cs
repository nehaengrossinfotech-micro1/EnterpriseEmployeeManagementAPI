using EnterpriseEmployeeManagementAPI.Data;
using EnterpriseEmployeeManagementAPI.HealthChecks;
using EnterpriseEmployeeManagementAPI.Logging;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Moq;
using Serilog;

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

    [Fact]
    public async Task DatabaseHealthCheckReportsUnhealthyWhenNoProviderIsConfigured()
    {
        await using var services = new ServiceCollection()
            .AddScoped(_ => new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>().Options))
            .BuildServiceProvider();
        var healthCheck = new DatabaseHealthCheck(
            services.GetRequiredService<IServiceScopeFactory>());

        var result = await healthCheck.CheckHealthAsync(
            new HealthCheckContext(),
            CancellationToken.None);

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Exception.Should().NotBeNull();
    }

    [Fact]
    public void LoggingConfigurationReturnsConfiguredLogger()
    {
        var loggerConfiguration = new LoggerConfiguration();
        var configuration = new ConfigurationBuilder().Build();
        var environment = new Mock<IHostEnvironment>(MockBehavior.Strict);
        environment
            .SetupGet(item => item.EnvironmentName)
            .Returns("Test");

        var result = LoggingConfiguration.Configure(
            loggerConfiguration,
            configuration,
            environment.Object);

        result.Should().BeSameAs(loggerConfiguration);
        environment.VerifyAll();
    }
}
