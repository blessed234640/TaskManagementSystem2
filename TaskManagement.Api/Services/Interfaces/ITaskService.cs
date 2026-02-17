// Контракт сервиса задач для использования в контроллере и DI.
using TaskManagement.Api.Domain.Entities;
using TaskManagement.Api.Domain.Enums;
using TaskStatusEnum = TaskManagement.Api.Domain.Enums.TaskStatus;
using TaskManagement.Api.Transport;

namespace TaskManagement.Api.Services.Interfaces;

public interface ITaskService
{
    Task<PagedResult<TaskItem>> GetTasksAsync(TaskFilter filter, CancellationToken cancellationToken);
    Task<TaskItem> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<TaskItem> CreateAsync(TaskCreateRequest request, int currentUserId, CancellationToken cancellationToken);
    Task<TaskItem> UpdateAsync(int id, TaskUpdateRequest request, int currentUserId, CancellationToken cancellationToken);
    Task<TaskItem> ChangeStatusAsync(int id, TaskStatusEnum status, int currentUserId, CancellationToken cancellationToken);
    Task<TaskItem> ChangePriorityAsync(int id, TaskPriority priority, int currentUserId, CancellationToken cancellationToken);
    Task<TaskItem> AssignAsync(int id, int assigneeUserId, int currentUserId, CancellationToken cancellationToken);
    Task DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken);
}

