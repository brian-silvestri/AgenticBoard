using AgenticBoard.Application.Common.Exceptions;
using AgenticBoard.Application.Common.Interfaces;
using AgenticBoard.Application.Features.Tasks.Dtos;
using AgenticBoard.Application.Features.Tasks.Services;
using AgenticBoard.Application.Features.Tasks.Validators;
using AgenticBoard.Domain.Entities;
using AgenticBoard.Domain.Enums;
using AgenticBoard.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AgenticBoard.Tests.Tasks;

public class TaskServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly CreateTaskDtoValidator _createValidator = new();
    private readonly UpdateTaskDtoValidator _updateValidator = new();
    private readonly UpdateTaskStatusDtoValidator _statusValidator = new();
    private readonly TaskService _taskService;

    private readonly User _memberUser;
    private readonly User _otherMember;
    private readonly User _outsiderUser;
    private readonly Project _project;

    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        _memberUser = new User { Id = 1, Email = "member@example.com", FullName = "Member User", PasswordHash = "hash" };
        _otherMember = new User { Id = 2, Email = "colleague@example.com", FullName = "Colleague User", PasswordHash = "hash" };
        _outsiderUser = new User { Id = 3, Email = "outsider@example.com", FullName = "Outsider User", PasswordHash = "hash" };

        _dbContext.Users.AddRange(_memberUser, _otherMember, _outsiderUser);

        _project = new Project { Id = 10, Name = "Sprint Workspace", CreatedById = _memberUser.Id };
        _dbContext.Projects.Add(_project);

        _dbContext.ProjectMembers.AddRange(
            new ProjectMember { ProjectId = _project.Id, UserId = _memberUser.Id, Role = ProjectRole.Owner },
            new ProjectMember { ProjectId = _project.Id, UserId = _otherMember.Id, Role = ProjectRole.Member }
        );

        _dbContext.SaveChanges();

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(new DateTime(2026, 9, 17, 15, 0, 0, DateTimeKind.Utc));
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns(_memberUser.Id);

        _taskService = new TaskService(
            _dbContext,
            _currentUserServiceMock.Object,
            _dateTimeProviderMock.Object,
            _createValidator,
            _updateValidator,
            _statusValidator
        );
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task CreateTaskAsync_WhenCallerIsMember_ShouldCreateTaskAndLogAudit()
    {
        // Arrange
        var request = new CreateTaskDto("Implement Drag and Drop", "Integrate Angular CDK", TaskPriority.High, _otherMember.Id);

        // Act
        var result = await _taskService.CreateTaskAsync(_project.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Implement Drag and Drop");
        result.Status.Should().Be(TaskItemStatus.Backlog);
        result.Priority.Should().Be(TaskPriority.High);
        result.AssignedUserId.Should().Be(_otherMember.Id);
        result.AssignedUserName.Should().Be("Colleague User");

        var audit = await _dbContext.AuditLogs.SingleOrDefaultAsync(a => a.ProjectId == _project.Id && a.Action == "TaskCreated");
        audit.Should().NotBeNull();
        audit!.NewValue.Should().Be("Implement Drag and Drop");
    }

    [Fact]
    public async Task CreateTaskAsync_WhenCallerIsNotMember_ShouldThrowForbiddenException()
    {
        // Arrange
        _currentUserServiceMock.Setup(c => c.UserId).Returns(_outsiderUser.Id);
        var request = new CreateTaskDto("Unauthorized Task", null, TaskPriority.Low, null);

        // Act
        var act = () => _taskService.CreateTaskAsync(_project.Id, request);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*access to this project*");
    }

    [Fact]
    public async Task CreateTaskAsync_WhenAssigneeIsNotProjectMember_ShouldThrowValidationException()
    {
        // Arrange: assign to outsider
        var request = new CreateTaskDto("Task with bad assignee", null, TaskPriority.Medium, _outsiderUser.Id);

        // Act
        var act = () => _taskService.CreateTaskAsync(_project.Id, request);

        // Assert
        var ex = await act.Should().ThrowAsync<AgenticBoard.Application.Common.Exceptions.ValidationException>();
        ex.Which.Errors.Should().ContainKey("AssignedUserId");
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_ShouldPersistNewStatus_AndCreateAuditLog()
    {
        // Arrange: create initial task in Backlog
        var task = new TaskItem
        {
            ProjectId = _project.Id,
            Title = "Status Transition Task",
            Status = TaskItemStatus.Backlog,
            Priority = TaskPriority.Medium,
            CreatedById = _memberUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateTaskStatusDto(TaskItemStatus.InProgress);

        // Act
        var result = await _taskService.UpdateTaskStatusAsync(task.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(TaskItemStatus.InProgress);

        var audit = await _dbContext.AuditLogs.FirstOrDefaultAsync(a => a.EntityId == task.Id.ToString() && a.Action == "StatusChanged");
        audit.Should().NotBeNull();
        audit!.OldValue.Should().Be("Backlog");
        audit.NewValue.Should().Be("InProgress");
    }

    [Fact]
    public async Task UpdateTaskAsync_WhenPriorityChanges_ShouldCreatePriorityAuditLog()
    {
        // Arrange: task with Low priority
        var task = new TaskItem
        {
            ProjectId = _project.Id,
            Title = "Priority Task",
            Status = TaskItemStatus.Todo,
            Priority = TaskPriority.Low,
            CreatedById = _memberUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateTaskDto("Priority Task", null, TaskItemStatus.Todo, TaskPriority.Critical, null);

        // Act
        var result = await _taskService.UpdateTaskAsync(task.Id, request);

        // Assert
        result.Priority.Should().Be(TaskPriority.Critical);

        var audit = await _dbContext.AuditLogs.FirstOrDefaultAsync(a => a.EntityId == task.Id.ToString() && a.Action == "PriorityChanged");
        audit.Should().NotBeNull();
        audit!.OldValue.Should().Be("Low");
        audit.NewValue.Should().Be("Critical");
    }
}
