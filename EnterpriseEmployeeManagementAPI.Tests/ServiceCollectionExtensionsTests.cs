using EnterpriseEmployeeManagementAPI.Data;
using EnterpriseEmployeeManagementAPI.Extensions;
using EnterpriseEmployeeManagementAPI.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseEmployeeManagementAPI.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddEmployeeModuleRegistersApplicationServices()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        services.AddEmployeeModule(configuration);

        services.Should().Contain(item => item.ServiceType == typeof(IEmployeeRepository));
        services.Should().Contain(item => item.ServiceType == typeof(IEmployeeService));
        services.Should().Contain(item => item.ServiceType == typeof(ApplicationDbContext));
    }

    [Fact]
    public void AddEmployeeModuleRequiresConnectionString()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        Action act = () => services.AddEmployeeModule(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*DefaultConnection*");
    }

    [Fact]
    public void AddEmployeeModuleRequiresJwtConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=Test",
            })
            .Build();

        Action act = () => services.AddEmployeeModule(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT configuration*");
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=Test",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:Secret"] = "test-only-secret-with-at-least-32-characters",
                ["Jwt:ExpirationMinutes"] = "60",
            })
            .Build();
    }
}
