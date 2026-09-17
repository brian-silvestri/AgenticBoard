using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AgenticBoard.Application.Features.Auth.Dtos;
using AgenticBoard.Application.Features.Projects.Dtos;
using AgenticBoard.Application.Features.Tasks.Dtos;
using AgenticBoard.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AgenticBoard.Tests.Integration;

public class TaskEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TaskEndpointsTests(CustomWebApplicationFactory factory)
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
    public async Task TaskWorkflow_Create_PatchStatus_EnforceSecurity_Success()
    {
        // 1. User A creates project
        var tokenA = await RegisterAndGetTokenAsync("task_owner@example.com", "Task Owner");
        var tokenOutsider = await RegisterAndGetTokenAsync("task_outsider@example.com", "Task Outsider");

        var createProjectMsg = new HttpRequestMessage(HttpMethod.Post, "/api/projects")
        {
            Content = JsonContent.Create(new CreateProjectDto("Kanban Testing Project", "Desc"))
        };
        createProjectMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var projRes = await _client.SendAsync(createProjectMsg);
        var project = await projRes.Content.ReadFromJsonAsync<ProjectDto>();

        // 2. User A creates task
        var createTaskMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/projects/{project!.Id}/tasks")
        {
            Content = JsonContent.Create(new CreateTaskDto("Build Drag and Drop Kanban", "Detailed spec", TaskPriority.High, null))
        };
        createTaskMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var taskRes = await _client.SendAsync(createTaskMsg);
        taskRes.StatusCode.Should().Be(HttpStatusCode.Created);

        var task = await taskRes.Content.ReadFromJsonAsync<TaskItemDto>();
        task.Should().NotBeNull();
        task!.Status.Should().Be(TaskItemStatus.Backlog);

        // 3. User A simulates Kanban drag & drop: moves from Backlog to InProgress
        var patchStatusMsg = new HttpRequestMessage(HttpMethod.Patch, $"/api/tasks/{task.Id}/status")
        {
            Content = JsonContent.Create(new UpdateTaskStatusDto(TaskItemStatus.InProgress))
        };
        patchStatusMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var patchRes = await _client.SendAsync(patchStatusMsg);
        patchRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var patchedTask = await patchRes.Content.ReadFromJsonAsync<TaskItemDto>();
        patchedTask!.Status.Should().Be(TaskItemStatus.InProgress);

        // 4. Outsider attempts to PATCH status (Must be 403 Forbidden)
        var outsiderPatchMsg = new HttpRequestMessage(HttpMethod.Patch, $"/api/tasks/{task.Id}/status")
        {
            Content = JsonContent.Create(new UpdateTaskStatusDto(TaskItemStatus.Done))
        };
        outsiderPatchMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenOutsider);
        var outsiderRes = await _client.SendAsync(outsiderPatchMsg);
        outsiderRes.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
