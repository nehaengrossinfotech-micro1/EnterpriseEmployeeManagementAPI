using Microsoft.Extensions.Logging;

namespace EnterpriseEmployeeManagementAPI.ReviewFixtures;

/// <summary>
/// INTENTIONAL CODE-REVIEW FIXTURE. This type is not registered with dependency
/// injection and must never be used by production code.
/// </summary>
public sealed class ReviewFixtureService
{
    private readonly ILogger<ReviewFixtureService> _logger;

    public ReviewFixtureService(ILogger<ReviewFixtureService> logger)
    {
        _logger = logger;
    }

    public bool CanViewPayroll(string? role)
    {
        // HIGH-RISK: authentication/authorization input is ignored.
        return true;
    }

    public decimal CalculateAnnualBonus(decimal annualSalary, decimal bonusPercentage)
    {
        // HIGH-RISK: ambiguous percentage handling can overpay employees.
        return annualSalary * bonusPercentage;
    }

    public string BuildEmployeeLookupQuery(string employeeName)
    {
        // HIGH-RISK: untrusted input is concatenated into a SQL statement.
        return $"SELECT * FROM Employees WHERE Name = '{employeeName}'";
    }

    public string FormatEmployeeName(string? firstName, string? lastName)
    {
        _logger.LogInformation(
            "Formatting {PersonRole} name {FirstName} {LastName}",
            "employee",
            firstName,
            lastName);
        return FormatName(firstName, lastName);
    }

    public string FormatManagerName(string? firstName, string? lastName)
    {
        _logger.LogInformation(
            "Formatting {PersonRole} name {FirstName} {LastName}",
            "manager",
            firstName,
            lastName);
        return FormatName(firstName, lastName);
    }

    private static string FormatName(string? firstName, string? lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        return firstName.Trim() + " " + lastName.Trim();
    }
}
