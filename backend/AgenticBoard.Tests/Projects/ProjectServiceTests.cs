using AgenticBoard.Application.Common.Exceptions;
using AgenticBoard.Application.Common.Interfaces;
using AgenticBoard.Application.Features.Projects.Dtos;
using AgenticBoard.Application.Features.Projects.Services;
using AgenticBoard.Application.Features.Projects.Validators;
using AgenticBoard.Domain.Entities;
using AgenticBoard.Domain.Enums;
using AgenticBoard.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AgenticBoard.Tests.Projects;

public class ProjectServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly CreateProjectDtoValidator _createValidator = new();
    private readonly UpdateProjectDtoValidator _updateValidator = new();
    private readonly AddProjectMemberDtoValidator _addMemberValidator = new();
    private readonly ProjectService _projectService;

    private readonly User _ownerUser;
    private readonly User _memberUser;
    private readonly User _outsiderUser;

    public ProjectServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        _ownerUser = new User { Id = 1, Email = "owner@example.com", FullName = "Owner User", PasswordHash = "hash" };
        _memberUser = new User { Id = 2, Email = "member@example.com", FullName = "Member User", PasswordHash = "hash" };
        _outsiderUser = new User { Id = 3, Email = "outsider@example.com", FullName = "Outsider User", PasswordHash = "hash" };

        _dbContext.Users.AddRange(_ownerUser, _memberUser, _outsiderUser);
        _dbContext.SaveChanges();

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(new DateTime(2026, 9, 17, 14, 0, 0, DateTimeKind.Utc));

        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns(_ownerUser.Id);

        _projectService = new ProjectService(
            _dbContext,
            _currentUserServiceMock.Object,
            _dateTimeProviderMock.Object,
            _createValidator,
            _updateValidator,
            _addMemberValidator
        );
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task CreateProjectAsync_ShouldSetCreatorAsOwner_AndGenerateAuditLog()
    {
        // Arrange
        var request = new CreateProjectDto("New Platform Project", "Project Description");

        // Act
        var result = await _projectService.CreateProjectAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Platform Project");
        result.MyRole.Should().Be(ProjectRole.Owner);

        var memberInDb = await _dbContext.ProjectMembers.SingleOrDefaultAsync(m => m.ProjectId == result.Id);
        memberInDb.Should().NotBeNull();
        memberInDb!.UserId.Should().Be(_ownerUser.Id);
        memberInDb.Role.Should().Be(ProjectRole.Owner);

        var auditLog = await _dbContext.AuditLogs.SingleOrDefaultAsync(a => a.ProjectId == result.Id);
        auditLog.Should().NotBeNull();
        auditLog!.Action.Should().Be("ProjectCreated");
    }

    [Fact]
    public async Task GetProjectByIdAsync_WhenCallerIsNotMember_ShouldThrowForbiddenException()
    {
        // Arrange: project with owner
        var project = new Project { Name = "Private Project", CreatedById = _ownerUser.Id };
        _dbContext.Projects.Add(project);
        _dbContext.ProjectMembers.Add(new ProjectMember { Project = project, UserId = _ownerUser.Id, Role = ProjectRole.Owner });
        await _dbContext.SaveChangesAsync();

        // Switch caller to outsider
        _currentUserServiceMock.Setup(c => c.UserId).Returns(_outsiderUser.Id);

        // Act
        var act = () => _projectService.GetProjectByIdAsync(project.Id);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*not a member*");
    }

    [Fact]
    public async Task AddMemberAsync_WhenCallerIsOwner_ShouldAddMemberAndLogAudit()
    {
        // Arrange
        var project = new Project { Name = "Team Project", CreatedById = _ownerUser.Id };
        _dbContext.Projects.Add(project);
        _dbContext.ProjectMembers.Add(new ProjectMember { Project = project, UserId = _ownerUser.Id, Role = ProjectRole.Owner });
        await _dbContext.SaveChangesAsync();

        var request = new AddProjectMemberDto("member@example.com", ProjectRole.Member);

        // Act
        var result = await _projectService.AddMemberAsync(project.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("member@example.com");
        result.Role.Should().Be(ProjectRole.Member);

        var audit = await _dbContext.AuditLogs.FirstOrDefaultAsync(a => a.ProjectId == project.Id && a.Action == "MemberAdded");
        audit.Should().NotBeNull();
    }

    [Fact]
    public async Task AddMemberAsync_WhenCallerIsNotOwner_ShouldThrowForbiddenException()
    {
        // Arrange: member is caller
        var project = new Project { Name = "Team Project", CreatedById = _ownerUser.Id };
        _dbContext.Projects.Add(project);
        _dbContext.ProjectMembers.Add(new ProjectMember { Project = project, UserId = _ownerUser.Id, Role = ProjectRole.Owner });
        _dbContext.ProjectMembers.Add(new ProjectMember { Project = project, UserId = _memberUser.Id, Role = ProjectRole.Member });
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock.Setup(c => c.UserId).Returns(_memberUser.Id);

        var request = new AddProjectMemberDto("outsider@example.com", ProjectRole.Member);

        // Act
        var act = () => _projectService.AddMemberAsync(project.Id, request);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*Only project owners*");
    }

    [Fact]
    public async Task AddMemberAsync_WhenTargetUserAlreadyMember_ShouldThrowConflictException()
    {
        // Arrange
        var project = new Project { Name = "Team Project", CreatedById = _ownerUser.Id };
        _dbContext.Projects.Add(project);
        _dbContext.ProjectMembers.Add(new ProjectMember { Project = project, UserId = _ownerUser.Id, Role = ProjectRole.Owner });
        _dbContext.ProjectMembers.Add(new ProjectMember { Project = project, UserId = _memberUser.Id, Role = ProjectRole.Member });
        await _dbContext.SaveChangesAsync();

        var request = new AddProjectMemberDto("member@example.com", ProjectRole.Member);

        // Act
        var act = () => _projectService.AddMemberAsync(project.Id, request);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already a member*");
    }

    [Fact]
    public async Task RemoveMemberAsync_WhenTargetIsSoleOwner_ShouldThrowValidationException()
    {
        // Arrange
        var project = new Project { Name = "Single Owner Project", CreatedById = _ownerUser.Id };
        _dbContext.Projects.Add(project);
        _dbContext.ProjectMembers.Add(new ProjectMember { Project = project, UserId = _ownerUser.Id, Role = ProjectRole.Owner });
        await _dbContext.SaveChangesAsync();

        // Act: owner tries to remove himself
        var act = () => _projectService.RemoveMemberAsync(project.Id, _ownerUser.Id);

        // Assert
        var exception = await act.Should().ThrowAsync<AgenticBoard.Application.Common.Exceptions.ValidationException>();
        exception.Which.Errors.Should().ContainKey("UserId");
        exception.Which.Errors["UserId"].Should().Contain("Cannot remove the only owner of the project.");
    }
}
