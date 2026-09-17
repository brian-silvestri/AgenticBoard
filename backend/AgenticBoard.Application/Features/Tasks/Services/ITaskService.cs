using AgenticBoard.Application.Features.Tasks.Dtos;
using AgenticBoard.Domain.Enums;

namespace AgenticBoard.Application.Features.Tasks.Services;

public interface ITaskService
{
    Task<TaskItemDto> CreateTaskAsync(int projectId, CreateTaskDto dto, CancellationToken cancellationToken = default);
    Task<List<TaskItemDto>> GetTasksByProjectAsync(int projectId, TaskItemStatus? status = null, int? assignedUserId = null, CancellationToken cancellationToken = default);
    Task<TaskItemDetailDto> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TaskItemDto> UpdateTaskAsync(int id, UpdateTaskDto dto, CancellationToken cancellationToken = default);
    Task<TaskItemDto> UpdateTaskStatusAsync(int id, UpdateTaskStatusDto dto, CancellationToken cancellationToken = default);
    Task<TaskItemDto> UpdateTaskAssignmentAsync(int id, UpdateTaskAssignmentDto dto, CancellationToken cancellationToken = default);
    Task DeleteTaskAsync(int id, CancellationToken cancellationToken = default);
}
