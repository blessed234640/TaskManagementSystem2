// Контроллер задач: CRUD, смена статуса/приоритета и назначение исполнителя.
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.Domain.Enums;
using TaskStatusEnum = TaskManagement.Api.Domain.Enums.TaskStatus;
using TaskManagement.Api.Services.Interfaces;
using TaskManagement.Api.Transport;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks([FromQuery] TaskFilter filter, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetTasksAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TaskCreateRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        try
        {
            var task = await _taskService.CreateAsync(request, currentUserId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TaskUpdateRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        try
        {
            var task = await _taskService.UpdateAsync(id, request, currentUserId, cancellationToken);
            return Ok(task);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] TaskStatusEnum status, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        try
        {
            var task = await _taskService.ChangeStatusAsync(id, status, currentUserId, cancellationToken);
            return Ok(task);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/priority")]
    public async Task<IActionResult> ChangePriority(int id, [FromBody] TaskPriority priority, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        try
        {
            var task = await _taskService.ChangePriorityAsync(id, priority, currentUserId, cancellationToken);
            return Ok(task);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] int assigneeUserId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        try
        {
            var task = await _taskService.AssignAsync(id, assigneeUserId, currentUserId, cancellationToken);
            return Ok(task);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        try
        {
            await _taskService.DeleteAsync(id, currentUserId, cancellationToken);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    private int GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name);
        if (!int.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user id in token");
        }

        return userId;
    }
}

