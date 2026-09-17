using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AgenticBoard.Application.Features.Auth.Dtos;
using AgenticBoard.Application.Features.Projects.Dtos;
using AgenticBoard.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AgenticBoard.Tests.Integration;

public class ProjectEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProjectEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndGetTokenAsync(string email, string name)
    {
        var reg = new RegisterDto(email, name, "P@ssword123!");
        var res = await _client.PostAsJsonAsync("/api/auth/register", reg);
        var data = await res.Content.ReadFromJsonAsync<AuthResponseDto>();
        return data!.Token;
    }

    [Fact]
    public async Task ProjectLifecycle_Create_Read_AndEnforceMembership_Success()
    {
        // 1. Create User A (Owner) and User B (Outsider)
        var tokenA = await RegisterAndGetTokenAsync("owner_proj@example.com", "Project Owner");
        var tokenB = await RegisterAndGetTokenAsync("outsider_proj@example.com", "Outsider User");

        // 2. User A creates project
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/projects")
        {
            Content = JsonContent.Create(new CreateProjectDto("Alpha Core", "Project Alpha Description"))
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var createResponse = await _client.SendAsync(createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
        createdProject.Should().NotBeNull();
        createdProject!.Name.Should().Be("Alpha Core");
        createdProject.MyRole.Should().Be(ProjectRole.Owner);

        // 3. User B attempts to read User A's project (Must be 403 Forbidden)
        var outsiderGetRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{createdProject.Id}");
        outsiderGetRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        var outsiderResponse = await _client.SendAsync(outsiderGetRequest);
        outsiderResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // 4. User A adds User B as Member
        var addMemberRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/projects/{createdProject.Id}/members")
        {
            Content = JsonContent.Create(new AddProjectMemberDto("outsider_proj@example.com", ProjectRole.Member))
        };
        addMemberRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var addMemberResponse = await _client.SendAsync(addMemberRequest);
        addMemberResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // 5. User B can now access the project
        var memberGetRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{createdProject.Id}");
        memberGetRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        var memberResponse = await _client.SendAsync(memberGetRequest);
        memberResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await memberResponse.Content.ReadFromJsonAsync<ProjectDetailDto>();
        detail.Should().NotBeNull();
        detail!.MyRole.Should().Be(ProjectRole.Member);
        detail.Members.Should().HaveCount(2);
    }
}
