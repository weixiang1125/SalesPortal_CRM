using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.Utils;
using Task = SharedLibrary.Models.Task;

namespace CRM_API.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Task>> GetTasksForUserAsync(int userId, bool isAdmin)
        {
            return await _context.DBTask
                .Include(t => t.Deal)
                .Include(t => t.Contact)
                .Where(t => t.CreatedBy == userId)
                .OrderByDescending(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<List<Task>> GetAllTasksFromDbAsync()
        {
            return await _context.DBTask
                .Include(t => t.Deal)
                .Include(t => t.Contact)
                .Include(t => t.CreatedByUser)
                .Include(t => t.UpdatedByUser)
                .OrderByDescending(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<Task?> GetTaskByIdAsync(int taskId)
        {
            return await _context.DBTask
                .Include(t => t.Deal)
                .Include(t => t.Contact)
                .FirstOrDefaultAsync(t => t.TaskID == taskId);
        }

        public async Task<Task> CreateTaskAsync(Task task)
        {
            task.CreatedDate = TimeHelper.Now();
            _context.DBTask.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<bool> UpdateTaskAsync(Task task, int updatedBy)
        {
            var existing = await _context.DBTask.FindAsync(task.TaskID);
            if (existing == null) return false;

            existing.TaskName = task.TaskName;
            existing.TaskDescription = task.TaskDescription;
            existing.DueDate = task.DueDate;
            existing.Status = task.Status;
            existing.DealID = task.DealID;
            existing.ContactID = task.ContactID;
            existing.UpdatedDate = TimeHelper.Now();
            existing.UpdatedBy = updatedBy;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var task = await _context.DBTask.FindAsync(taskId);
            if (task == null) return false;

            _context.DBTask.Remove(task);
            return await _context.SaveChangesAsync() > 0;
        }
    }

}
