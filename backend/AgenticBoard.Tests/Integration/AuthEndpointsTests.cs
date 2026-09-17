using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AgenticBoard.Application.Features.Auth.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace AgenticBoard.Tests.Integration;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_Returns201Created_AndToken()
    {
        // Arrange
        var request = new RegisterDto("integration_user@example.com", "Integration Tester", "P@ssword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.User.Email.Should().Be("integration_user@example.com");
    }

    [Fact]
    public async Task Register_WithExistingEmail_Returns409Conflict_WithProblemDetails()
    {
        // Arrange
        var request = new RegisterDto("duplicate_integration@example.com", "First User", "P@ssword123!");
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Act: try to register identical email
        var duplicateResponse = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problem = await duplicateResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Conflict");
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200OK_AndToken()
    {
        // Arrange
        var regRequest = new RegisterDto("login_test@example.com", "Login Tester", "P@ssword123!");
        await _client.PostAsJsonAsync("/api/auth/register", regRequest);

        var loginRequest = new LoginDto("login_test@example.com", "P@ssword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401Unauthorized_WithProblemDetails()
    {
        // Arrange
        var loginRequest = new LoginDto("demo@agenticboard.local", "WrongPassword!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_Returns200OK_AndProfile()
    {
        // Arrange: register to get a token
        var regRequest = new RegisterDto("token_me@example.com", "Token User", "P@ssword123!");
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", regRequest);
        var authData = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authData!.Token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var profile = await response.Content.ReadFromJsonAsync<UserDto>();
        profile.Should().NotBeNull();
        profile!.Email.Should().Be("token_me@example.com");
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_Returns401Unauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
