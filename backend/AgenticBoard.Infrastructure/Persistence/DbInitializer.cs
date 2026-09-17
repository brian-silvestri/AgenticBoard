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
        // 1. Connection retry loop for containerized/remote database initialization
        var maxRetries = 10;
        var delay = TimeSpan.FromSeconds(3);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                logger.LogInformation("Attempting to connect and apply database migrations (Attempt {Attempt}/{MaxRetries})...", attempt, maxRetries);

                if (context.Database.IsRelational())
                {
                    await context.Database.MigrateAsync(cancellationToken);
                }
                else
                {
                    await context.Database.EnsureCreatedAsync(cancellationToken);
                }

                logger.LogInformation("Database migration completed successfully.");
                break;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                logger.LogWarning(ex, "Database connection not ready yet. Retrying in {DelaySeconds}s...", delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to apply database migrations after {MaxRetries} attempts.", maxRetries);
                throw;
            }
        }

        // 2. Check if already seeded
        if (await context.Users.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Database already seeded. Skipping initial seeding.");
            return;
        }

        logger.LogInformation("Seeding development initial data...");

        // 3. Seed Users
        var demoUser = new User
        {
            Email = "demo@agenticboard.local",
            FullName = "Demo User",
            PasswordHash = passwordHasher.HashPassword("DemoPassword123!"),
            CreatedAt = DateTime.UtcNow
        };

        var alexUser = new User
        {
            Email = "alex@agenticboard.dev",
            FullName = "Alex Morgan",
            PasswordHash = passwordHasher.HashPassword("Pass123!"),
            CreatedAt = DateTime.UtcNow
        };

        var jordanUser = new User
        {
            Email = "jordan@agenticboard.dev",
            FullName = "Jordan Lee",
            PasswordHash = passwordHasher.HashPassword("Pass123!"),
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(demoUser, alexUser, jordanUser);
        await context.SaveChangesAsync(cancellationToken);

        // 4. Sample Project
        var project = new Project
        {
            Name = "AgenticBoard MVP",
            Description = "Core system development demonstrating Spec-Driven and AI-First full-stack engineering.",
            CreatedById = alexUser.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync(cancellationToken);

        // 5. Project Members
        var ownerMembership = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = alexUser.Id,
            Role = ProjectRole.Owner,
            JoinedAt = DateTime.UtcNow.AddDays(-5)
        };

        var memberMembership1 = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = jordanUser.Id,
            Role = ProjectRole.Member,
            JoinedAt = DateTime.UtcNow.AddDays(-4)
        };

        var memberMembership2 = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = demoUser.Id,
            Role = ProjectRole.Member,
            JoinedAt = DateTime.UtcNow.AddDays(-3)
        };

        context.ProjectMembers.AddRange(ownerMembership, memberMembership1, memberMembership2);

        // 6. Sample Tasks across all Kanban statuses
        var task1 = new TaskItem
        {
            ProjectId = project.Id,
            Title = "Set up CI/CD pipeline with GitHub Actions",
            Description = "Configure automated workflows for building backend and running tests on PRs.",
            Status = TaskItemStatus.Done,
            Priority = TaskPriority.High,
            CreatedById = alexUser.Id,
            AssignedUserId = alexUser.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        var task2 = new TaskItem
        {
            ProjectId = project.Id,
            Title = "Implement JWT Bearer Authentication & Claims",
            Description = "Secure endpoints with signed JSON Web Tokens and claims-based identity resolution.",
            Status = TaskItemStatus.InProgress,
            Priority = TaskPriority.Critical,
            CreatedById = alexUser.Id,
            AssignedUserId = jordanUser.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var task3 = new TaskItem
        {
            ProjectId = project.Id,
            Title = "Design Interactive Kanban Board with Angular CDK",
            Description = "Enable column drag-and-drop state transitions with visual status indicators.",
            Status = TaskItemStatus.Todo,
            Priority = TaskPriority.High,
            CreatedById = alexUser.Id,
            AssignedUserId = alexUser.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var task4 = new TaskItem
        {
            ProjectId = project.Id,
            Title = "Implement Project Audit Trail & Activity Feed",
            Description = "Record state changes automatically into tamper-evident audit entity.",
            Status = TaskItemStatus.Backlog,
            Priority = TaskPriority.Medium,
            CreatedById = jordanUser.Id,
            AssignedUserId = jordanUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        context.Tasks.AddRange(task1, task2, task3, task4);
        await context.SaveChangesAsync(cancellationToken);

        // 7. Sample Comments
        var comment1 = new TaskComment
        {
            TaskItemId = task2.Id,
            AuthorId = alexUser.Id,
            Text = "Tokens are configured with 24-hour expiration for development. Ready for review.",
            CreatedAt = DateTime.UtcNow.AddHours(-6)
        };

        var comment2 = new TaskComment
        {
            TaskItemId = task2.Id,
            AuthorId = jordanUser.Id,
            Text = "Verified claims extraction and interceptor token attachment. Looks solid!",
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        context.TaskComments.AddRange(comment1, comment2);

        // 8. Initial Audit Log
        var auditLogs = new List<AuditLog>
        {
            new AuditLog
            {
                ProjectId = project.Id,
                EntityType = "Project",
                EntityId = project.Id.ToString(),
                Action = "ProjectCreated",
                NewValue = project.Name,
                PerformedById = alexUser.Id,
                PerformedByName = alexUser.FullName,
                Timestamp = DateTime.UtcNow.AddDays(-5)
            },
            new AuditLog
            {
                ProjectId = project.Id,
                EntityType = "TaskItem",
                EntityId = task1.Id.ToString(),
                Action = "TaskCreated",
                NewValue = task1.Title,
                PerformedById = alexUser.Id,
                PerformedByName = alexUser.FullName,
                Timestamp = DateTime.UtcNow.AddDays(-3)
            },
            new AuditLog
            {
                ProjectId = project.Id,
                EntityType = "TaskItem",
                EntityId = task2.Id.ToString(),
                Action = "TaskCreated",
                NewValue = task2.Title,
                PerformedById = alexUser.Id,
                PerformedByName = alexUser.FullName,
                Timestamp = DateTime.UtcNow.AddDays(-2)
            },
            new AuditLog
            {
                ProjectId = project.Id,
                EntityType = "TaskItem",
                EntityId = task2.Id.ToString(),
                Action = "CommentAdded",
                NewValue = "Tokens are configured with 24-hour expiration...",
                PerformedById = alexUser.Id,
                PerformedByName = alexUser.FullName,
                Timestamp = DateTime.UtcNow.AddHours(-6)
            }
        };

        context.AuditLogs.AddRange(auditLogs);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Database seeded successfully with initial users: alex@agenticboard.dev, jordan@agenticboard.dev, demo@agenticboard.local");
    }
}
