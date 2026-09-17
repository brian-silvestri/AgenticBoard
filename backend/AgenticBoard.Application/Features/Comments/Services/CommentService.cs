using AgenticBoard.Application.Common.Exceptions;
using AgenticBoard.Application.Common.Interfaces;
using AgenticBoard.Application.Features.Comments.Dtos;
using AgenticBoard.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AgenticBoard.Application.Features.Comments.Services;

public class CommentService : ICommentService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IValidator<CreateCommentDto> _validator;

    public CommentService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IValidator<CreateCommentDto> validator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _validator = validator;
    }

    private int GetCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException("User must be authenticated.");
        }
        return _currentUserService.UserId.Value;
    }

    public async Task<CommentDto> AddCommentAsync(int taskId, CreateCommentDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Common.Exceptions.ValidationException(validationResult.Errors);
        }

        var currentUserId = GetCurrentUserId();

        var task = await _context.Tasks
            .Include(t => t.Project)
            .SingleOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null)
        {
            throw new NotFoundException(nameof(TaskItem), taskId);
        }

        var isMember = await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUserId, cancellationToken);

        if (!isMember)
        {
            throw new ForbiddenException("You are not a member of the project associated with this task.");
        }

        var caller = await _context.Users.FindAsync(new object[] { currentUserId }, cancellationToken)
                     ?? throw new NotFoundException(nameof(User), currentUserId);

        var now = _dateTimeProvider.UtcNow;
        var comment = new TaskComment
        {
            TaskItemId = taskId,
            AuthorId = currentUserId,
            Text = dto.Text.Trim(),
            CreatedAt = now
        };

        _context.TaskComments.Add(comment);

        // Audit Trail
        _context.AuditLogs.Add(new AuditLog
        {
            ProjectId = task.ProjectId,
            EntityType = "Task",
            EntityId = task.Id.ToString(),
            Action = "CommentAdded",
            NewValue = dto.Text.Length > 80 ? dto.Text.Substring(0, 80) + "..." : dto.Text,
            PerformedById = currentUserId,
            PerformedByName = caller.FullName,
            Timestamp = now
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new CommentDto(
            comment.Id,
            comment.TaskItemId,
            comment.AuthorId,
            caller.FullName,
            comment.Text,
            comment.CreatedAt
        );
    }

    public async Task<List<CommentDto>> GetCommentsByTaskIdAsync(int taskId, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        var task = await _context.Tasks
            .SingleOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null)
        {
            throw new NotFoundException(nameof(TaskItem), taskId);
        }

        var isMember = await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUserId, cancellationToken);

        if (!isMember)
        {
            throw new ForbiddenException("You are not a member of the project associated with this task.");
        }

        var comments = await _context.TaskComments
            .AsNoTracking()
            .Where(c => c.TaskItemId == taskId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto(
                c.Id,
                c.TaskItemId,
                c.AuthorId,
                c.Author != null ? c.Author.FullName : "User",
                c.Text,
                c.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return comments;
    }
}
