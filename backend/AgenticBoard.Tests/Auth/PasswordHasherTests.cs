using AgenticBoard.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace AgenticBoard.Tests.Auth;

public class PasswordHasherTests
{
    private readonly BcryptPasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_ShouldReturnSaltedHash_DifferentFromPlaintext()
    {
        // Arrange
        const string password = "SecurePassword123!";

        // Act
        var hash = _hasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(password);
        hash.Should().StartWith("$2");
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        const string password = "CorrectPassword123!";
        var hash = _hasher.HashPassword(password);

        // Act
        var isValid = _hasher.VerifyPassword(password, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        const string password = "CorrectPassword123!";
        var hash = _hasher.HashPassword(password);

        // Act
        var isValid = _hasher.VerifyPassword("WrongPassword123!", hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("", "some-hash")]
    [InlineData("password", "")]
    [InlineData("password", "not-a-valid-bcrypt-hash")]
    public void VerifyPassword_WithMalformedOrEmptyInputs_ShouldReturnFalse(string password, string hash)
    {
        // Act
        var isValid = _hasher.VerifyPassword(password, hash);

        // Assert
        isValid.Should().BeFalse();
    }
}
