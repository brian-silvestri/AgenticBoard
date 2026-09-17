using AgenticBoard.Application.Common.Exceptions;
using AgenticBoard.Application.Common.Interfaces;
using AgenticBoard.Application.Features.Audit.Services;
using AgenticBoard.Domain.Entities;
using AgenticBoard.Domain.Enums;
using AgenticBoard.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AgenticBoard.Tests.Audit;

public class AuditLogServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly AuditLogService _auditService;

    private readonly User _memberUser;
    private readonly User _outsiderUser;
    private readonly Project _project;

    public AuditLogServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        _memberUser = new User { Id = 1, Email = "auditor@example.com", FullName = "Auditor User", PasswordHash = "hash" };
        _outsiderUser = new User { Id = 2, Email = "outsider@example.com", FullName = "Outsider User", PasswordHash = "hash" };
        _dbContext.Users.AddRange(_memberUser, _outsiderUser);

        _project = new Project { Id = 50, Name = "Audit Project", CreatedById = _memberUser.Id };
        _dbContext.Projects.Add(_project);
        _dbContext.ProjectMembers.Add(new ProjectMember { ProjectId = _project.Id, UserId = _memberUser.Id, Role = ProjectRole.Owner });

        _dbContext.AuditLogs.AddRange(
            new AuditLog
            {
                ProjectId = _project.Id,
                EntityType = "Project",
                EntityId = _project.Id.ToString(),
                Action = "ProjectCreated",
                Timestamp = DateTime.UtcNow.AddHours(-2)
            },
            new AuditLog
            {
                ProjectId = _project.Id,
                EntityType = "Task",
                EntityId = "101",
                Action = "StatusChanged",
                OldValue = "InProgress",
                NewValue = "Done",
                Timestamp = DateTime.UtcNow.AddHours(-1)
            }
        );

        _dbContext.SaveChanges();

        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns(_memberUser.Id);

        _auditService = new AuditLogService(_dbContext, _currentUserServiceMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task GetProjectActivityAsync_WhenCallerIsMember_ShouldReturnOrderedEvents()
    {
        // Act
        var result = await _auditService.GetProjectActivityAsync(_project.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Action.Should().Be("StatusChanged"); // Newest first
        result[1].Action.Should().Be("ProjectCreated");
    }

    [Fact]
    public async Task GetProjectActivityAsync_WhenCallerIsNotMember_ShouldThrowForbiddenException()
    {
        // Arrange
        _currentUserServiceMock.Setup(c => c.UserId).Returns(_outsiderUser.Id);

        // Act
        var act = () => _auditService.GetProjectActivityAsync(_project.Id);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
