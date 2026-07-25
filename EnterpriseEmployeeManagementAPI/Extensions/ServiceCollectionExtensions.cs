using System.Text;
using EnterpriseEmployeeManagementAPI.Configuration;
using EnterpriseEmployeeManagementAPI.Data;
using EnterpriseEmployeeManagementAPI.HealthChecks;
using EnterpriseEmployeeManagementAPI.Interfaces;
using EnterpriseEmployeeManagementAPI.Repositories;
using EnterpriseEmployeeManagementAPI.Services;
using EnterpriseEmployeeManagementAPI.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EnterpriseEmployeeManagementAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmployeeModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is required.");
        var jwtSettings = configuration
            .GetRequiredSection(JwtSettings.SectionName)
            .Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT configuration is required.");

        services
            .AddOptions<JwtSettings>()
            .Bind(configuration.GetRequiredSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlServer => sqlServer.EnableRetryOnFailure()));

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddValidatorsFromAssemblyContaining<CreateEmployeeRequestValidator>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });
        services.AddAuthorization();
        services
            .AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

        return services;
    }
}
