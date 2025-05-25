using CRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            foreach (var claim in User.Claims)
                Console.WriteLine($"[CLAIM] {claim.Type} = {claim.Value}");

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
            var task = await _taskService.GetTaskByIdAsync(id);
            return task != null ? Ok(task) : NotFound();
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
