using AgenticBoard.Application.Common.Exceptions;
using AgenticBoard.Application.Common.Interfaces;
using AgenticBoard.Application.Features.Projects.Dtos;
using AgenticBoard.Domain.Entities;
using AgenticBoard.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AgenticBoard.Application.Features.Projects.Services;

public class ProjectService : IProjectService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IValidator<CreateProjectDto> _createValidator;
    private readonly IValidator<UpdateProjectDto> _updateValidator;
    private readonly IValidator<AddProjectMemberDto> _addMemberValidator;

    public ProjectService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IValidator<CreateProjectDto> createValidator,
        IValidator<UpdateProjectDto> updateValidator,
        IValidator<AddProjectMemberDto> addMemberValidator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _addMemberValidator = addMemberValidator;
    }

    private int GetCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException("User must be authenticated.");
        }
        return _currentUserService.UserId.Value;
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Common.Exceptions.ValidationException(validationResult.Errors);
        }

        var currentUserId = GetCurrentUserId();
        var currentUser = await _context.Users.FindAsync(new object[] { currentUserId }, cancellationToken)
                          ?? throw new NotFoundException(nameof(User), currentUserId);

        var now = _dateTimeProvider.UtcNow;
        var project = new Project
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            CreatedById = currentUserId,
            CreatedAt = now
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        // Assign creator as Owner
        var membership = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = currentUserId,
            Role = ProjectRole.Owner,
            JoinedAt = now
        };

        _context.ProjectMembers.Add(membership);

        // Audit Trail
        var audit = new AuditLog
        {
            ProjectId = project.Id,
            EntityType = "Project",
            EntityId = project.Id.ToString(),
            Action = "ProjectCreated",
            NewValue = project.Name,
            PerformedById = currentUserId,
            PerformedByName = currentUser.FullName,
            Timestamp = now
        };

        _context.AuditLogs.Add(audit);
        await _context.SaveChangesAsync(cancellationToken);

        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.CreatedById,
            currentUser.FullName,
            project.CreatedAt,
            ProjectRole.Owner,
            1,
            0
        );
    }

    public async Task<List<ProjectDto>> GetMyProjectsAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        var projects = await _context.Projects
            .AsNoTracking()
            .Where(p => p.Members.Any(m => m.UserId == currentUserId))
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectDto(
                p.Id,
                p.Name,
                p.Description,
                p.CreatedById,
                p.CreatedBy != null ? p.CreatedBy.FullName : "System",
                p.CreatedAt,
                p.Members.Where(m => m.UserId == currentUserId).Select(m => m.Role).FirstOrDefault(),
                p.Members.Count,
                p.Tasks.Count
            ))
            .ToListAsync(cancellationToken);

        return projects;
    }

    public async Task<ProjectDetailDto> GetProjectByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        var project = await _context.Projects
            .AsNoTracking()
            .Include(p => p.CreatedBy)
            .Include(p => p.Members)
                .ThenInclude(m => m.User)
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null)
        {
            throw new NotFoundException(nameof(Project), id);
        }

        var callerMember = project.Members.FirstOrDefault(m => m.UserId == currentUserId);
        if (callerMember == null)
        {
            throw new ForbiddenException("You are not a member of this project.");
        }

        var membersDto = project.Members
            .Select(m => new ProjectMemberDto(
                m.UserId,
                m.User.Email,
                m.User.FullName,
                m.Role,
                m.JoinedAt
            ))
            .OrderBy(m => m.Role)
            .ThenBy(m => m.FullName)
            .ToList();

        return new ProjectDetailDto(
            project.Id,
            project.Name,
            project.Description,
            project.CreatedById,
            project.CreatedBy?.FullName ?? "System",
            project.CreatedAt,
            callerMember.Role,
            membersDto
        );
    }

    public async Task<ProjectDto> UpdateProjectAsync(int id, UpdateProjectDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Common.Exceptions.ValidationException(validationResult.Errors);
        }

        var currentUserId = GetCurrentUserId();

        var project = await _context.Projects
            .Include(p => p.CreatedBy)
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null)
        {
            throw new NotFoundException(nameof(Project), id);
        }

        var callerMember = project.Members.FirstOrDefault(m => m.UserId == currentUserId);
        if (callerMember == null || callerMember.Role != ProjectRole.Owner)
        {
            throw new ForbiddenException("Only project owners can modify project details.");
        }

        var now = _dateTimeProvider.UtcNow;
        var oldName = project.Name;

        project.Name = dto.Name.Trim();
        project.Description = dto.Description?.Trim();
        project.UpdatedAt = now;

        if (oldName != project.Name)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                ProjectId = project.Id,
                EntityType = "Project",
                EntityId = project.Id.ToString(),
                Action = "ProjectUpdated",
                OldValue = oldName,
                NewValue = project.Name,
                PerformedById = currentUserId,
                Timestamp = now
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.CreatedById,
            project.CreatedBy?.FullName ?? "System",
            project.CreatedAt,
            callerMember.Role,
            project.Members.Count,
            project.Tasks.Count
        );
    }

    public async Task<ProjectMemberDto> AddMemberAsync(int projectId, AddProjectMemberDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _addMemberValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Common.Exceptions.ValidationException(validationResult.Errors);
        }

        var currentUserId = GetCurrentUserId();

        var project = await _context.Projects
            .Include(p => p.Members)
            .SingleOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            throw new NotFoundException(nameof(Project), projectId);
        }

        var callerMember = project.Members.FirstOrDefault(m => m.UserId == currentUserId);
        if (callerMember == null || callerMember.Role != ProjectRole.Owner)
        {
            throw new ForbiddenException("Only project owners can add members.");
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var targetUser = await _context.Users
            .SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (targetUser == null)
        {
            throw new NotFoundException($"User with email '{dto.Email}' was not found in the system.");
        }

        if (project.Members.Any(m => m.UserId == targetUser.Id))
        {
            throw new ConflictException($"User '{targetUser.FullName}' is already a member of this project.");
        }

        var now = _dateTimeProvider.UtcNow;
        var newMember = new ProjectMember
        {
            ProjectId = projectId,
            UserId = targetUser.Id,
            Role = dto.Role,
            JoinedAt = now
        };

        _context.ProjectMembers.Add(newMember);

        // Audit Trail
        _context.AuditLogs.Add(new AuditLog
        {
            ProjectId = projectId,
            EntityType = "ProjectMember",
            EntityId = targetUser.Id.ToString(),
            Action = "MemberAdded",
            NewValue = $"{targetUser.FullName} ({dto.Role})",
            PerformedById = currentUserId,
            Timestamp = now
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new ProjectMemberDto(
            targetUser.Id,
            targetUser.Email,
            targetUser.FullName,
            newMember.Role,
            newMember.JoinedAt
        );
    }

    public async Task RemoveMemberAsync(int projectId, int userId, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        var project = await _context.Projects
            .Include(p => p.Members)
                .ThenInclude(m => m.User)
            .SingleOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            throw new NotFoundException(nameof(Project), projectId);
        }

        var callerMember = project.Members.FirstOrDefault(m => m.UserId == currentUserId);
        if (callerMember == null || callerMember.Role != ProjectRole.Owner)
        {
            throw new ForbiddenException("Only project owners can remove members.");
        }

        var targetMember = project.Members.FirstOrDefault(m => m.UserId == userId);
        if (targetMember == null)
        {
            throw new NotFoundException("Member does not exist in this project.");
        }

        // Prevent removing the sole owner
        if (targetMember.Role == ProjectRole.Owner && project.Members.Count(m => m.Role == ProjectRole.Owner) <= 1)
        {
            throw new Common.Exceptions.ValidationException("UserId", "Cannot remove the only owner of the project.");
        }

        // Unassign any tasks assigned to this user in this project
        var tasksAssigned = await _context.Tasks
            .Where(t => t.ProjectId == projectId && t.AssignedUserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var task in tasksAssigned)
        {
            task.AssignedUserId = null;
        }

        _context.ProjectMembers.Remove(targetMember);

        // Audit Trail
        _context.AuditLogs.Add(new AuditLog
        {
            ProjectId = projectId,
            EntityType = "ProjectMember",
            EntityId = userId.ToString(),
            Action = "MemberRemoved",
            OldValue = targetMember.User.FullName,
            PerformedById = currentUserId,
            Timestamp = _dateTimeProvider.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
