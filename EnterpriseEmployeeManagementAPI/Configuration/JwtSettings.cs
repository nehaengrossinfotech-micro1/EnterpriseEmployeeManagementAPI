namespace EnterpriseEmployeeManagementAPI.Configuration;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public required string Issuer { get; init; }

    public required string Audience { get; init; }

    public required string Secret { get; init; }

    public int ExpirationMinutes { get; init; } = 60;
}
