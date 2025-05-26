using CRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.DTOs;
using SharedLibrary.Utils;
using Task = SharedLibrary.Models.Task;

namespace CRM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : BaseController
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService, ILogger<BaseController> logger)
            : base(logger)
        {
            _taskService = taskService;
        }

        [Authorize]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _taskService.GetTasksForUserAsync(CurrentUserId, IsAdmin);
            return Ok(tasks);
        }

        [HttpGet("GetAllTasksFromDb")]
        public async Task<IActionResult> GetAllTasksFromDb()
        {
            var allTasks = await _taskService.GetAllTasksFromDbAsync();
            return Ok(allTasks);
        }

        [Authorize]
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var t = await _taskService.GetTaskByIdAsync(id);
            if (t == null)
                return NotFound();

            var dto = new TaskDto
            {
                TaskID = t.TaskID,
                TaskName = t.TaskName,
                TaskDescription = t.TaskDescription,
                DueDate = t.DueDate,
                Status = t.Status,
                ContactID = t.ContactID,
                DealID = t.DealID,
                ContactName = t.Contact?.Name,
                DealName = t.Deal?.DealName
            };

            return Ok(dto);
        }

        [Authorize]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] Task task)
        {
            task.CreatedBy = CurrentUserId;
            task.CreatedDate = TimeHelper.Now();

            var created = await _taskService.CreateTaskAsync(task);
            return CreatedAtAction(nameof(GetById), new { id = created.TaskID }, created);
        }

        [Authorize]
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Task task)
        {
            if (id != task.TaskID)
                return BadRequest("Task ID mismatch");

            var success = await _taskService.UpdateTaskAsync(task, CurrentUserId);
            return success ? NoContent() : NotFound();
        }

        [Authorize]
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _taskService.DeleteTaskAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
