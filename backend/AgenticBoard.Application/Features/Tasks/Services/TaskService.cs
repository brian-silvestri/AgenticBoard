using AgenticBoard.Application.Common.Exceptions;
using AgenticBoard.Application.Common.Interfaces;
using AgenticBoard.Application.Features.Tasks.Dtos;
using AgenticBoard.Domain.Entities;
using AgenticBoard.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AgenticBoard.Application.Features.Tasks.Services;

public class TaskService : ITaskService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IValidator<CreateTaskDto> _createValidator;
    private readonly IValidator<UpdateTaskDto> _updateValidator;
    private readonly IValidator<UpdateTaskStatusDto> _statusValidator;

    public TaskService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IValidator<CreateTaskDto> createValidator,
        IValidator<UpdateTaskDto> updateValidator,
        IValidator<UpdateTaskStatusDto> statusValidator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _statusValidator = statusValidator;
    }

    private int GetCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException("User must be authenticated.");
        }
        return _currentUserService.UserId.Value;
    }

    private async Task VerifyProjectMembershipAsync(int projectId, int userId, CancellationToken cancellationToken)
    {
        var isMember = await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId, cancellationToken);

        if (!isMember)
        {
            throw new ForbiddenException("You do not have access to this project.");
        }
    }

    public async Task<TaskItemDto> CreateTaskAsync(int projectId, CreateTaskDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Common.Exceptions.ValidationException(validationResult.Errors);
        }

        var currentUserId = GetCurrentUserId();
        await VerifyProjectMembershipAsync(projectId, currentUserId, cancellationToken);

        string? assignedUserName = null;
        if (dto.AssignedUserId.HasValue)
        {
            var assigneeMember = await _context.ProjectMembers
                .Include(pm => pm.User)
                .SingleOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == dto.AssignedUserId.Value, cancellationToken);

            if (assigneeMember == null)
            {
                throw new Common.Exceptions.ValidationException("AssignedUserId", "The assigned user must be an active member of this project.");
            }
            assignedUserName = assigneeMember.User.FullName;
        }

        var currentUser = await _context.Users.FindAsync(new object[] { currentUserId }, cancellationToken)
                          ?? throw new NotFoundException(nameof(User), currentUserId);

        var now = _dateTimeProvider.UtcNow;
        var task = new TaskItem
        {
            ProjectId = projectId,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Status = TaskItemStatus.Backlog,
            Priority = dto.Priority,
            AssignedUserId = dto.AssignedUserId,
            CreatedById = currentUserId,
            CreatedAt = now
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        // Audit Trail
        _context.AuditLogs.Add(new AuditLog
        {
            ProjectId = projectId,
            EntityType = "Task",
            EntityId = task.Id.ToString(),
            Action = "TaskCreated",
            NewValue = task.Title,
            PerformedById = currentUserId,
            PerformedByName = currentUser.FullName,
            Timestamp = now
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new TaskItemDto(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.AssignedUserId,
            assignedUserName,
            task.CreatedById,
            currentUser.FullName,
            task.CreatedAt,
            task.UpdatedAt,
            0
        );
    }

    public async Task<List<TaskItemDto>> GetTasksByProjectAsync(
        int projectId,
        TaskItemStatus? status = null,
        int? assignedUserId = null,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        await VerifyProjectMembershipAsync(projectId, currentUserId, cancellationToken);

        var query = _context.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId);

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (assignedUserId.HasValue)
        {
            query = query.Where(t => t.AssignedUserId == assignedUserId.Value);
        }

        var tasks = await query
            .OrderBy(t => t.Status)
            .ThenByDescending(t => t.CreatedAt)
            .Select(t => new TaskItemDto(
                t.Id,
                t.ProjectId,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.AssignedUserId,
                t.AssignedUser != null ? t.AssignedUser.FullName : null,
                t.CreatedById,
                t.CreatedBy != null ? t.CreatedBy.FullName : "System",
                t.CreatedAt,
                t.UpdatedAt,
                t.Comments.Count
            ))
            .ToListAsync(cancellationToken);

        return tasks;
    }

    public async Task<TaskItemDetailDto> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        var task = await _context.Tasks
            .AsNoTracking()
            .Include(t => t.AssignedUser)
            .Include(t => t.CreatedBy)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task == null)
        {
            throw new NotFoundException(nameof(TaskItem), id);
        }

        await VerifyProjectMembershipAsync(task.ProjectId, currentUserId, cancellationToken);

        var commentsDto = task.Comments
            .OrderBy(c => c.CreatedAt)
            .Select(c => new TaskCommentDto(
                c.Id,
                c.TaskItemId,
                c.AuthorId,
                c.Author?.FullName ?? "User",
                c.Text,
                c.CreatedAt
            ))
            .ToList();

        return new TaskItemDetailDto(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.AssignedUserId,
            task.AssignedUser?.FullName,
            task.CreatedById,
            task.CreatedBy?.FullName ?? "System",
            task.CreatedAt,
            task.UpdatedAt,
            commentsDto
        );
    }

    public async Task<TaskItemDto> UpdateTaskAsync(int id, UpdateTaskDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Common.Exceptions.ValidationException(validationResult.Errors);
        }

        var currentUserId = GetCurrentUserId();

        var task = await _context.Tasks
            .Include(t => t.AssignedUser)
            .Include(t => t.CreatedBy)
            .Include(t => t.Comments)
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task == null)
        {
            throw new NotFoundException(nameof(TaskItem), id);
        }

        await VerifyProjectMembershipAsync(task.ProjectId, currentUserId, cancellationToken);

        string? assignedUserName = null;
        if (dto.AssignedUserId.HasValue)
        {
            var assigneeMember = await _context.ProjectMembers
                .Include(pm => pm.User)
                .SingleOrDefaultAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == dto.AssignedUserId.Value, cancellationToken);

            if (assigneeMember == null)
            {
                throw new Common.Exceptions.ValidationException("AssignedUserId", "The assigned user must be an active member of this project.");
            }
            assignedUserName = assigneeMember.User.FullName;
        }

        var now = _dateTimeProvider.UtcNow;
        var caller = await _context.Users.FindAsync(new object[] { currentUserId }, cancellationToken);
        var callerName = caller?.FullName ?? "System";

        // Audit Status change
        if (task.Status != dto.Status)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                ProjectId = task.ProjectId,
                EntityType = "Task",
                EntityId = task.Id.ToString(),
                Action = "StatusChanged",
                OldValue = task.Status.ToString(),
                NewValue = dto.Status.ToString(),
                PerformedById = currentUserId,
                PerformedByName = callerName,
                Timestamp = now
            });
            task.Status = dto.Status;
        }

        // Audit Priority change
        if (task.Priority != dto.Priority)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                ProjectId = task.ProjectId,
                EntityType = "Task",
                EntityId = task.Id.ToString(),
                Action = "PriorityChanged",
                OldValue = task.Priority.ToString(),
                NewValue = dto.Priority.ToString(),
                PerformedById = currentUserId,
                PerformedByName = callerName,
                Timestamp = now
            });
            task.Priority = dto.Priority;
        }

        // Audit Assignment change
        if (task.AssignedUserId != dto.AssignedUserId)
        {
            var oldAssignee = task.AssignedUser?.FullName ?? "Unassigned";
            var newAssignee = assignedUserName ?? "Unassigned";

            _context.AuditLogs.Add(new AuditLog
            {
                ProjectId = task.ProjectId,
                EntityType = "Task",
                EntityId = task.Id.ToString(),
                Action = "AssignedUserChanged",
                OldValue = oldAssignee,
                NewValue = newAssignee,
                PerformedById = currentUserId,
                PerformedByName = callerName,
                Timestamp = now
            });
            task.AssignedUserId = dto.AssignedUserId;
        }

        task.Title = dto.Title.Trim();
        task.Description = dto.Description?.Trim();
        task.UpdatedAt = now;

        await _context.SaveChangesAsync(cancellationToken);

        return new TaskItemDto(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.AssignedUserId,
            assignedUserName,
            task.CreatedById,
            task.CreatedBy?.FullName ?? "System",
            task.CreatedAt,
            task.UpdatedAt,
            task.Comments.Count
        );
    }

    public async Task<TaskItemDto> UpdateTaskStatusAsync(int id, UpdateTaskStatusDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _statusValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Common.Exceptions.ValidationException(validationResult.Errors);
        }

        var currentUserId = GetCurrentUserId();

        var task = await _context.Tasks
            .Include(t => t.AssignedUser)
            .Include(t => t.CreatedBy)
            .Include(t => t.Comments)
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task == null)
        {
            throw new NotFoundException(nameof(TaskItem), id);
        }

        await VerifyProjectMembershipAsync(task.ProjectId, currentUserId, cancellationToken);

        if (task.Status != dto.Status)
        {
            var now = _dateTimeProvider.UtcNow;
            var caller = await _context.Users.FindAsync(new object[] { currentUserId }, cancellationToken);

            _context.AuditLogs.Add(new AuditLog
            {
                ProjectId = task.ProjectId,
                EntityType = "Task",
                EntityId = task.Id.ToString(),
                Action = "StatusChanged",
                OldValue = task.Status.ToString(),
                NewValue = dto.Status.ToString(),
                PerformedById = currentUserId,
                PerformedByName = caller?.FullName ?? "System",
                Timestamp = now
            });

            task.Status = dto.Status;
            task.UpdatedAt = now;

            await _context.SaveChangesAsync(cancellationToken);
        }

        return new TaskItemDto(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.AssignedUserId,
            task.AssignedUser?.FullName,
            task.CreatedById,
            task.CreatedBy?.FullName ?? "System",
            task.CreatedAt,
            task.UpdatedAt,
            task.Comments.Count
        );
    }

    public async Task<TaskItemDto> UpdateTaskAssignmentAsync(int id, UpdateTaskAssignmentDto dto, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        var task = await _context.Tasks
            .Include(t => t.AssignedUser)
            .Include(t => t.CreatedBy)
            .Include(t => t.Comments)
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task == null)
        {
            throw new NotFoundException(nameof(TaskItem), id);
        }

        await VerifyProjectMembershipAsync(task.ProjectId, currentUserId, cancellationToken);

        string? newAssigneeName = null;
        if (dto.AssignedUserId.HasValue)
        {
            var assigneeMember = await _context.ProjectMembers
                .Include(pm => pm.User)
                .SingleOrDefaultAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == dto.AssignedUserId.Value, cancellationToken);

            if (assigneeMember == null)
            {
                throw new Common.Exceptions.ValidationException("AssignedUserId", "The assigned user must be an active member of this project.");
            }
            newAssigneeName = assigneeMember.User.FullName;
        }

        if (task.AssignedUserId != dto.AssignedUserId)
        {
            var now = _dateTimeProvider.UtcNow;
            var caller = await _context.Users.FindAsync(new object[] { currentUserId }, cancellationToken);

            _context.AuditLogs.Add(new AuditLog
            {
                ProjectId = task.ProjectId,
                EntityType = "Task",
                EntityId = task.Id.ToString(),
                Action = "AssignedUserChanged",
                OldValue = task.AssignedUser?.FullName ?? "Unassigned",
                NewValue = newAssigneeName ?? "Unassigned",
                PerformedById = currentUserId,
                PerformedByName = caller?.FullName ?? "System",
                Timestamp = now
            });

            task.AssignedUserId = dto.AssignedUserId;
            task.UpdatedAt = now;

            await _context.SaveChangesAsync(cancellationToken);
        }

        return new TaskItemDto(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.AssignedUserId,
            newAssigneeName,
            task.CreatedById,
            task.CreatedBy?.FullName ?? "System",
            task.CreatedAt,
            task.UpdatedAt,
            task.Comments.Count
        );
    }

    public async Task DeleteTaskAsync(int id, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        var task = await _context.Tasks
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task == null)
        {
            throw new NotFoundException(nameof(TaskItem), id);
        }

        await VerifyProjectMembershipAsync(task.ProjectId, currentUserId, cancellationToken);

        _context.AuditLogs.Add(new AuditLog
        {
            ProjectId = task.ProjectId,
            EntityType = "Task",
            EntityId = task.Id.ToString(),
            Action = "TaskDeleted",
            OldValue = task.Title,
            PerformedById = currentUserId,
            Timestamp = _dateTimeProvider.UtcNow
        });

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
