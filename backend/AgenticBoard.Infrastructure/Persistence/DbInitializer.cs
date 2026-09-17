using AgenticBoard.Application.Common.Interfaces;
using AgenticBoard.Domain.Entities;
using AgenticBoard.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgenticBoard.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (context.Database.IsRelational())
            {
                await context.Database.MigrateAsync(cancellationToken);
            }
            else
            {
                await context.Database.EnsureCreatedAsync(cancellationToken);
            }

            if (await context.Users.AnyAsync(cancellationToken))
            {
                logger.LogInformation("Database already seeded. Skipping initial seeding.");
                return;
            }

            logger.LogInformation("Seeding development initial data...");

            // 1. Users
            var demoUser = new User
            {
                Email = "demo@agenticboard.local",
                FullName = "Demo User",
                PasswordHash = passwordHasher.HashPassword("DemoPassword123!"),
                CreatedAt = DateTime.UtcNow
            };

            var alexUser = new User
            {
                Email = "alex@agenticboard.local",
                FullName = "Alex Developer",
                PasswordHash = passwordHasher.HashPassword("DemoPassword123!"),
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(demoUser, alexUser);
            await context.SaveChangesAsync(cancellationToken);

            // 2. Sample Project
            var project = new Project
            {
                Name = "AgenticBoard MVP",
                Description = "Core system development demonstrating Spec-Driven and AI-First full-stack engineering.",
                CreatedById = demoUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Projects.Add(project);
            await context.SaveChangesAsync(cancellationToken);

            // 3. Project Members
            var ownerMembership = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = demoUser.Id,
                Role = ProjectRole.Owner,
                JoinedAt = DateTime.UtcNow
            };

            var memberMembership = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = alexUser.Id,
                Role = ProjectRole.Member,
                JoinedAt = DateTime.UtcNow
            };

            context.ProjectMembers.AddRange(ownerMembership, memberMembership);

            // 4. Sample Tasks across statuses
            var task1 = new TaskItem
            {
                ProjectId = project.Id,
                Title = "Set up CI/CD pipeline with GitHub Actions",
                Description = "Configure automated workflows for building backend and running tests on PRs.",
                Status = TaskItemStatus.Done,
                Priority = TaskPriority.High,
                CreatedById = demoUser.Id,
                AssignedUserId = demoUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            var task2 = new TaskItem
            {
                ProjectId = project.Id,
                Title = "Implement JWT Bearer Authentication & Claims",
                Description = "Secure endpoints with signed JSON Web Tokens and claims-based identity resolution.",
                Status = TaskItemStatus.InProgress,
                Priority = TaskPriority.Critical,
                CreatedById = demoUser.Id,
                AssignedUserId = alexUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var task3 = new TaskItem
            {
                ProjectId = project.Id,
                Title = "Design Interactive Kanban Board with Angular CDK",
                Description = "Enable column drag-and-drop state transitions with visual status indicators.",
                Status = TaskItemStatus.Todo,
                Priority = TaskPriority.High,
                CreatedById = demoUser.Id,
                AssignedUserId = demoUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            var task4 = new TaskItem
            {
                ProjectId = project.Id,
                Title = "Implement Project Audit Trail Logging",
                Description = "Record state changes automatically into tamper-evident audit entity.",
                Status = TaskItemStatus.Backlog,
                Priority = TaskPriority.Medium,
                CreatedById = alexUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Tasks.AddRange(task1, task2, task3, task4);
            await context.SaveChangesAsync(cancellationToken);

            // 5. Sample Comments
            var comment = new TaskComment
            {
                TaskItemId = task2.Id,
                AuthorId = demoUser.Id,
                Text = "Tokens are configured with 24-hour expiration for development. Ready for review.",
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            };

            context.TaskComments.Add(comment);

            // 6. Initial Audit Log
            var auditLog = new AuditLog
            {
                ProjectId = project.Id,
                EntityType = "Project",
                EntityId = project.Id.ToString(),
                Action = "ProjectCreated",
                NewValue = project.Name,
                PerformedById = demoUser.Id,
                PerformedByName = demoUser.FullName,
                Timestamp = DateTime.UtcNow.AddDays(-2)
            };

            context.AuditLogs.Add(auditLog);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Database seeded successfully with demo user demo@agenticboard.local and initial workspace.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
