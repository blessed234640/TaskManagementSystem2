// DTO для операций логина и ответа с токеном.
namespace TaskManagement.Api.Transport;

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginResponse
{
    public string Token { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string RoleCode { get; set; } = null!;
}

