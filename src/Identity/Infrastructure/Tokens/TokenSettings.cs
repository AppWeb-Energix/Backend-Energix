namespace Energix.API.Identity.Infrastructure.Tokens;

/// <summary>
/// JWT Token configuration settings
/// </summary>
public class TokenSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "energix";
    public string Audience { get; set; } = "energix-client";
    public int ExpiresMinutes { get; set; } = 60;
}

