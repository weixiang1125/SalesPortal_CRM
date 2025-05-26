using SharedLibrary.DTOs;
using Task = SharedLibrary.Models.Task;

namespace CRM_API.Services
{
    public interface ITaskService
    {
        Task<List<TaskDto>> GetTasksForUserAsync(int userId, bool isAdmin);
        Task<Task> CreateTaskAsync(Task task);
        Task<bool> UpdateTaskAsync(Task task, int updatedBy);
        Task<bool> DeleteTaskAsync(int taskId);
        Task<Task?> GetTaskByIdAsync(int taskId);
        Task<List<Task>> GetAllTasksFromDbAsync();

    }

}
