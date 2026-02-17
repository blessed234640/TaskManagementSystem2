// Бизнес-логика работы с задачами с учетом ролевой модели и прав.
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Domain.Entities;
using TaskManagement.Api.Domain.Enums;
using TaskStatusEnum = TaskManagement.Api.Domain.Enums.TaskStatus;
using TaskManagement.Api.Infrastructure;
using TaskManagement.Api.Services.Interfaces;
using TaskManagement.Api.Transport;

namespace TaskManagement.Api.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _db;

    public TaskService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<TaskItem>> GetTasksAsync(TaskFilter filter, CancellationToken cancellationToken)
    {
        var query = _db.Tasks
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .AsQueryable();

        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filter.Priority.Value);
        }

        if (filter.DepartmentId.HasValue)
        {
            query = query.Where(t => t.AssignedToUser.DepartmentId == filter.DepartmentId.Value);
        }

        query = query.OrderByDescending(t => t.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskItem>
        {
            Items = items,
            TotalCount = total
        };
    }

    public async Task<TaskItem> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var task = await _db.Tasks
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        return task;
    }

    public async Task<TaskItem> CreateAsync(TaskCreateRequest request, int currentUserId, CancellationToken cancellationToken)
    {
        var currentUser = await _db.Users.Include(u => u.Role).FirstAsync(u => u.Id == currentUserId, cancellationToken);

        EnsureHasPermission(currentUser, "TASK_CREATE");

        var assignedUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.AssignedToUserId, cancellationToken);
        if (assignedUser == null)
        {
            throw new ArgumentException("Assignee user not found");
        }

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Status = TaskStatusEnum.New,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = currentUserId,
            AssignedToUserId = request.AssignedToUserId
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<TaskItem> UpdateAsync(int id, TaskUpdateRequest request, int currentUserId, CancellationToken cancellationToken)
    {
        var currentUser = await _db.Users.Include(u => u.Role).FirstAsync(u => u.Id == currentUserId, cancellationToken);
        var task = await GetByIdAsync(id, cancellationToken);

        if (currentUser.Role.Code == "EMPLOYEE" && task.AssignedToUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Employee can edit only own tasks");
        }

        if (currentUser.Role.Code == "VIEWER")
        {
            throw new UnauthorizedAccessException("Viewer cannot edit tasks");
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<TaskItem> ChangeStatusAsync(int id, TaskStatusEnum status, int currentUserId, CancellationToken cancellationToken)
    {
        var currentUser = await _db.Users.Include(u => u.Role).FirstAsync(u => u.Id == currentUserId, cancellationToken);
        var task = await GetByIdAsync(id, cancellationToken);

        if (currentUser.Role.Code == "EMPLOYEE" && task.AssignedToUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Employee can change status only for own tasks");
        }

        if (currentUser.Role.Code == "VIEWER")
        {
            throw new UnauthorizedAccessException("Viewer cannot change task status");
        }

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;
        if (status == TaskStatusEnum.Done)
        {
            task.CompletedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<TaskItem> ChangePriorityAsync(int id, TaskPriority priority, int currentUserId, CancellationToken cancellationToken)
    {
        var currentUser = await _db.Users.Include(u => u.Role).FirstAsync(u => u.Id == currentUserId, cancellationToken);
        var task = await GetByIdAsync(id, cancellationToken);

        if (currentUser.Role.Code != "MANAGER")
        {
            throw new UnauthorizedAccessException("Only manager can change priority");
        }

        task.Priority = priority;
        task.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<TaskItem> AssignAsync(int id, int assigneeUserId, int currentUserId, CancellationToken cancellationToken)
    {
        var currentUser = await _db.Users.Include(u => u.Role).FirstAsync(u => u.Id == currentUserId, cancellationToken);
        var task = await GetByIdAsync(id, cancellationToken);

        if (currentUser.Role.Code != "MANAGER")
        {
            throw new UnauthorizedAccessException("Only manager can assign tasks");
        }

        var assignee = await _db.Users.FirstOrDefaultAsync(u => u.Id == assigneeUserId, cancellationToken);
        if (assignee == null)
        {
            throw new ArgumentException("Assignee user not found");
        }

        task.AssignedToUserId = assigneeUserId;
        task.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken)
    {
        var currentUser = await _db.Users.Include(u => u.Role).FirstAsync(u => u.Id == currentUserId, cancellationToken);
        var task = await GetByIdAsync(id, cancellationToken);

        if (currentUser.Role.Code != "MANAGER")
        {
            throw new UnauthorizedAccessException("Only manager can delete tasks");
        }

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureHasPermission(User user, string permissionCode)
    {
        if (user.Role.Code == "MANAGER")
        {
            return;
        }

        throw new UnauthorizedAccessException("Insufficient permissions");
    }
}

