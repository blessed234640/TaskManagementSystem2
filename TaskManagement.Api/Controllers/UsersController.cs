// Контроллер пользователей: получение списков пользователей и по отделу.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Infrastructure;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _db.Users
            .Include(u => u.Department)
            .Include(u => u.Role)
            .ToListAsync(cancellationToken);

        return Ok(users);
    }

    [HttpGet("by-department/{departmentId:int}")]
    public async Task<IActionResult> GetByDepartment(int departmentId, CancellationToken cancellationToken)
    {
        var users = await _db.Users
            .Include(u => u.Department)
            .Include(u => u.Role)
            .Where(u => u.DepartmentId == departmentId)
            .ToListAsync(cancellationToken);

        return Ok(users);
    }
}

