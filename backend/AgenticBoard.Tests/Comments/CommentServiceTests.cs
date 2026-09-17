using AgenticBoard.Application.Common.Exceptions;
using AgenticBoard.Application.Common.Interfaces;
using AgenticBoard.Application.Features.Comments.Dtos;
using AgenticBoard.Application.Features.Comments.Services;
using AgenticBoard.Application.Features.Comments.Validators;
using AgenticBoard.Domain.Entities;
using AgenticBoard.Domain.Enums;
using AgenticBoard.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AgenticBoard.Tests.Comments;

public class CommentServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly CreateCommentDtoValidator _validator = new();
    private readonly CommentService _commentService;

    private readonly User _memberUser;
    private readonly User _outsiderUser;
    private readonly Project _project;
    private readonly TaskItem _task;

    public CommentServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        _memberUser = new User { Id = 1, Email = "member@example.com", FullName = "Member User", PasswordHash = "hash" };
        _outsiderUser = new User { Id = 2, Email = "outsider@example.com", FullName = "Outsider User", PasswordHash = "hash" };
        _dbContext.Users.AddRange(_memberUser, _outsiderUser);

        _project = new Project { Id = 10, Name = "Comment Test Project", CreatedById = _memberUser.Id };
        _dbContext.Projects.Add(_project);
        _dbContext.ProjectMembers.Add(new ProjectMember { ProjectId = _project.Id, UserId = _memberUser.Id, Role = ProjectRole.Owner });

        _task = new TaskItem { Id = 100, ProjectId = _project.Id, Title = "Comment Target Task", CreatedById = _memberUser.Id };
        _dbContext.Tasks.Add(_task);

        _dbContext.SaveChanges();

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(new DateTime(2026, 9, 17, 16, 0, 0, DateTimeKind.Utc));
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns(_memberUser.Id);

        _commentService = new CommentService(
            _dbContext,
            _currentUserServiceMock.Object,
            _dateTimeProviderMock.Object,
            _validator
        );
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task AddCommentAsync_WhenCallerIsMember_ShouldAddCommentAndAuditLog()
    {
        // Arrange
        var request = new CreateCommentDto("This is a technical update on the task implementation.");

        // Act
        var result = await _commentService.AddCommentAsync(_task.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Text.Should().Be("This is a technical update on the task implementation.");
        result.AuthorName.Should().Be("Member User");

        var commentInDb = await _dbContext.TaskComments.SingleOrDefaultAsync(c => c.Id == result.Id);
        commentInDb.Should().NotBeNull();

        var audit = await _dbContext.AuditLogs.FirstOrDefaultAsync(a => a.Action == "CommentAdded" && a.EntityId == _task.Id.ToString());
        audit.Should().NotBeNull();
    }

    [Fact]
    public async Task AddCommentAsync_WhenCallerIsNotMember_ShouldThrowForbiddenException()
    {
        // Arrange
        _currentUserServiceMock.Setup(c => c.UserId).Returns(_outsiderUser.Id);
        var request = new CreateCommentDto("Unauthorized comment");

        // Act
        var act = () => _commentService.AddCommentAsync(_task.Id, request);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddCommentAsync_WithEmptyComment_ShouldThrowValidationException(string text)
    {
        // Arrange
        var request = new CreateCommentDto(text);

        // Act
        var act = () => _commentService.AddCommentAsync(_task.Id, request);

        // Assert
        await act.Should().ThrowAsync<AgenticBoard.Application.Common.Exceptions.ValidationException>();
    }
}
