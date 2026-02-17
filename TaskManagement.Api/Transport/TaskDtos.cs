// DTO и фильтры для работы с задачами через API.
using TaskManagement.Api.Domain.Enums;
using TaskStatusEnum = TaskManagement.Api.Domain.Enums.TaskStatus;

namespace TaskManagement.Api.Transport;

public class TaskCreateRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public int AssignedToUserId { get; set; }
}

public class TaskUpdateRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
}

public class TaskFilter
{
    public TaskStatusEnum? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public int? DepartmentId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
}

