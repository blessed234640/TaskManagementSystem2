// Конфигурация параметров JWT (issuer, audience, ключ, время жизни).
namespace TaskManagement.Api.Auth;

public class JwtSettings
{
    public string Issuer { get; set; } = "TaskManagement.Api";
    public string Audience { get; set; } = "TaskManagement.Client";
    public string SecretKey { get; set; } = "super_secret_development_key_change_me";
    public int ExpirationMinutes { get; set; } = 120;
}

