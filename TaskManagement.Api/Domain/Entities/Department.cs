// Сущность отдела, к которому привязаны пользователи.
namespace TaskManagement.Api.Domain.Entities;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = new List<User>();
}

