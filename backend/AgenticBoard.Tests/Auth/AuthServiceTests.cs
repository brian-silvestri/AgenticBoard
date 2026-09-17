using AgenticBoard.Application.Common.Exceptions;
using AgenticBoard.Application.Common.Interfaces;
using AgenticBoard.Application.Features.Auth.Dtos;
using AgenticBoard.Application.Features.Auth.Services;
using AgenticBoard.Application.Features.Auth.Validators;
using AgenticBoard.Domain.Entities;
using AgenticBoard.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AgenticBoard.Tests.Auth;

public class AuthServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly RegisterDtoValidator _registerValidator = new();
    private readonly LoginDtoValidator _loginValidator = new();
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(new DateTime(2026, 9, 17, 12, 0, 0, DateTimeKind.Utc));
        _jwtTokenGeneratorMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("mock-jwt-token");
        _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashed-password");

        _authService = new AuthService(
            _dbContext,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _currentUserServiceMock.Object,
            _dateTimeProviderMock.Object,
            _registerValidator,
            _loginValidator);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task RegisterAsync_WithValidDetails_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var request = new RegisterDto("newuser@example.com", "John Doe", "StrongPass123!");

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("mock-jwt-token");
        result.User.Email.Should().Be("newuser@example.com");
        result.User.FullName.Should().Be("John Doe");

        var userInDb = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == "newuser@example.com");
        userInDb.Should().NotBeNull();
        userInDb!.PasswordHash.Should().Be("hashed-password");
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldThrowConflictException()
    {
        // Arrange
        var existingUser = new User
        {
            Email = "existing@example.com",
            FullName = "Existing User",
            PasswordHash = "hash"
        };
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        var request = new RegisterDto("existing@example.com", "Duplicate Person", "StrongPass123!");

        // Act
        var act = () => _authService.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");
    }

    [Theory]
    [InlineData("", "John Doe", "StrongPass123!")] // empty email
    [InlineData("invalid-email", "John Doe", "StrongPass123!")] // invalid email
    [InlineData("test@example.com", "", "StrongPass123!")] // empty name
    [InlineData("test@example.com", "John", "short")] // short password without uppercase/digit
    public async Task RegisterAsync_WithInvalidInput_ShouldThrowValidationException(string email, string name, string password)
    {
        // Arrange
        var request = new RegisterDto(email, name, password);

        // Act
        var act = () => _authService.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<AgenticBoard.Application.Common.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnTokenAndProfile()
    {
        // Arrange
        var user = new User
        {
            Email = "member@example.com",
            FullName = "Team Member",
            PasswordHash = "stored-hash"
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _passwordHasherMock.Setup(p => p.VerifyPassword("ValidPass123!", "stored-hash")).Returns(true);

        var request = new LoginDto("member@example.com", "ValidPass123!");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("mock-jwt-token");
        result.User.Email.Should().Be("member@example.com");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = new User
        {
            Email = "member@example.com",
            FullName = "Team Member",
            PasswordHash = "stored-hash"
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _passwordHasherMock.Setup(p => p.VerifyPassword("WrongPass!", "stored-hash")).Returns(false);

        var request = new LoginDto("member@example.com", "WrongPass!");

        // Act
        var act = () => _authService.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var request = new LoginDto("nonexistent@example.com", "AnyPassword123!");

        // Act
        var act = () => _authService.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenAuthenticated_ShouldReturnUserProfile()
    {
        // Arrange
        var user = new User
        {
            Email = "active@example.com",
            FullName = "Active User",
            PasswordHash = "hash"
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns(user.Id);

        // Act
        var result = await _authService.GetCurrentUserAsync();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be("active@example.com");
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(false);
        _currentUserServiceMock.Setup(c => c.UserId).Returns((int?)null);

        // Act
        var act = () => _authService.GetCurrentUserAsync();

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
