// Начальное заполнение БД ролями, пользователями, департаментами и примерной задачей.
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Domain.Entities;
using TaskManagement.Api.Domain.Enums;
using TaskStatusEnum = TaskManagement.Api.Domain.Enums.TaskStatus;

namespace TaskManagement.Api.Infrastructure;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (await db.Roles.AnyAsync())
        {
            return;
        }

        var managerRole = new Role { Name = "Начальник", Code = "MANAGER", Description = "Руководитель" };
        var employeeRole = new Role { Name = "Сотрудник", Code = "EMPLOYEE", Description = "Исполнитель" };
        var viewerRole = new Role { Name = "Наблюдатель", Code = "VIEWER", Description = "Только просмотр" };

        db.Roles.AddRange(managerRole, employeeRole, viewerRole);

        db.Permissions.AddRange(
            // Начальник
            new Permission { Code = "TASK_CREATE", Description = "Создание задач", Role = managerRole },
            new Permission { Code = "TASK_EDIT", Description = "Редактирование задач", Role = managerRole },
            new Permission { Code = "TASK_CHANGE_STATUS", Description = "Смена статуса задач", Role = managerRole },
            new Permission { Code = "TASK_CHANGE_PRIORITY", Description = "Смена приоритета задач", Role = managerRole },
            new Permission { Code = "TASK_ASSIGN", Description = "Назначение исполнителя", Role = managerRole },
            new Permission { Code = "TASK_COMPLETE", Description = "Завершение задач", Role = managerRole },
            new Permission { Code = "TASK_DELETE", Description = "Удаление задач", Role = managerRole },

            // Сотрудник
            new Permission { Code = "TASK_EDIT_ASSIGNED", Description = "Редактирование назначенных задач", Role = employeeRole },
            new Permission { Code = "TASK_CHANGE_STATUS_ASSIGNED", Description = "Смена статуса назначенных задач", Role = employeeRole },
            new Permission { Code = "TASK_COMPLETE_ASSIGNED", Description = "Завершение назначенных задач", Role = employeeRole },

            // Наблюдатель
            new Permission { Code = "TASK_VIEW", Description = "Просмотр задач", Role = viewerRole }
        );

        var depIt = new Department { Name = "ИТ", Description = "Отдел разработки", IsActive = true };
        var depHr = new Department { Name = "HR", Description = "Отдел кадров", IsActive = true };
        db.Departments.AddRange(depIt, depHr);

        var manager = new User
        {
            FullName = "Иван Иванов",
            Email = "manager@example.com",
            PasswordHash = PasswordHasher.HashPassword("manager123"),
            Department = depIt,
            Role = managerRole
        };

        var employee = new User
        {
            FullName = "Петр Петров",
            Email = "employee@example.com",
            PasswordHash = PasswordHasher.HashPassword("employee123"),
            Department = depIt,
            Role = employeeRole
        };

        var viewer = new User
        {
            FullName = "Сидор Сидоров",
            Email = "viewer@example.com",
            PasswordHash = PasswordHasher.HashPassword("viewer123"),
            Department = depHr,
            Role = viewerRole
        };

        db.Users.AddRange(manager, employee, viewer);

        db.Tasks.Add(new TaskItem
        {
            Title = "Пример задачи",
            Description = "Это тестовая задача",
            Status = TaskStatusEnum.New,
            Priority = TaskPriority.Medium,
            CreatedByUser = manager,
            AssignedToUser = employee
        });

        await db.SaveChangesAsync();
    }
}

