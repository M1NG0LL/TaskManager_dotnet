using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Data;
using TaskManagerAPI.Models;
using System.Security.Claims;
using TaskManagerAPI.Token;
using Task = TaskManagerAPI.Models.Task;

namespace TaskManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly Context _context;

        private Guid GetUserIdFromToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userId);
        }

        public TaskController(Context context)
        {
            _context = context;
        }

        /*
            POST FUNC
            api/Task/create 
        */
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateTask([FromBody] Task task)
        {
            task.AccountId = GetUserIdFromToken();
            task.Date = DateTime.UtcNow;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task created successfully." });
        }

        /*
            GET FUNC
            api/Task
        */
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllTasks()
        {
            var userId = GetUserIdFromToken();
            var tasks = await _context.Tasks.Where(t => t.AccountId == userId).ToListAsync();

            return Ok(tasks);
        }

        /*
            GET FUNC
            api/Task/{id}
        */
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetTaskById(Guid id)
        {
            var userId = GetUserIdFromToken();
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Uuid == id && t.AccountId == userId);

            if (task == null)
                return NotFound("Task not found.");

            return Ok(task);
        }

        /*
            PUT FUNC
            api/Task/{id}
        */
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] Task newtask)
        {
            var userId = GetUserIdFromToken();
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Uuid == id && t.AccountId == userId);

            if (task == null)
                return NotFound("Task not found.");

            task.Title = newtask.Title;
            task.Description = newtask.Description;

            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task updated successfully." });
        }

        /*
            DELETE FUNC
            api/Task/{id}
        */
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var userId = GetUserIdFromToken();
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Uuid == id && t.AccountId == userId);

            if (task == null)
                return NotFound("Task not found.");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task deleted successfully." });
        }
    }
}