using System.ComponentModel.DataAnnotations;

namespace AcademicManager.Web.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(32)]
    public string Key { get; init; } = string.Empty;

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Range(5, 1440)]
    public int ExpiryMinutes { get; init; } = 120;

    [Range(60, 43200)]
    public int RefreshExpiryMinutes { get; init; } = 10080;
}
