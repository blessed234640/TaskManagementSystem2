// Сущность разрешения, привязанного к роли.
namespace TaskManagement.Api.Domain.Entities;

public class Permission
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string? Description { get; set; }

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}

