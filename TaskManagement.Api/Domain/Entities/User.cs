// Сущность пользователя системы, привязанного к отделу и роли.
namespace TaskManagement.Api.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;

    // Для простоты тестового задания
    public string PasswordHash { get; set; } = null!;

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
}

