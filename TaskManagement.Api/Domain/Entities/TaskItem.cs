// Сущность задачи: статусы, приоритет, автор и исполнитель.
using TaskManagement.Api.Domain.Enums;
using TaskStatusEnum = TaskManagement.Api.Domain.Enums.TaskStatus;

namespace TaskManagement.Api.Domain.Entities;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public TaskStatusEnum Status { get; set; } = TaskStatusEnum.New;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public int AssignedToUserId { get; set; }
    public User AssignedToUser { get; set; } = null!;
}

