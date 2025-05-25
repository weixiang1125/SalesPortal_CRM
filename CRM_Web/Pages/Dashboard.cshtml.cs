using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Task = SharedLibrary.Models.Task;

namespace CRM_Web.Pages.Dashboard//  Make sure this matches your file structure!
{
    public class DashboardModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DashboardModel> _logger;
        public List<Task> Tasks { get; set; } = new();
        public Dictionary<string, List<Task>> GroupedTasks { get; set; } = new();
        public List<Task> TodayOrOverdueTasks { get; set; } = new();

        public DashboardModel(IHttpClientFactory factory, IHttpContextAccessor accessor, IConfiguration configuration, ILogger<DashboardModel> logger)
        {
            _clientFactory = factory;
            _httpContextAccessor = accessor;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogWarning("OnGetAsync hit!");

            var userId = HttpContext.Session.GetString("UserID");
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Login");

            ViewData["Username"] = username;
            ViewData["Role"] = role;

            var client = _clientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            client.BaseAddress = new Uri(apiBaseUrl);
            var token = HttpContext.Request.Cookies["authToken"];
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await client.GetAsync("api/task/GetAll");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var allTasks = JsonConvert.DeserializeObject<List<Task>>(json);
                    _logger.LogWarning("Tasks loaded: {Count}", allTasks.Count);



                    var today = DateTime.Today;
                    TodayOrOverdueTasks = allTasks
                        .Where(t =>
                            t.DueDate.HasValue &&
                            t.DueDate.Value.Date <= today &&
                            t.Status != "Completed")
                        .ToList();

                    _logger.LogWarning("TodayOrOverdueTasks count: {Count}", TodayOrOverdueTasks.Count);

                    // Add this block to build the reminder content
                    if (TodayOrOverdueTasks.Any())
                    {
                        var taskDetails = TodayOrOverdueTasks
                            .Select(t => $"- <strong>{t.TaskName}</strong>: {t.TaskDescription}")
                            .ToList();

                        ViewData["TaskReminder"] = string.Join("<br/>", taskDetails);
                    }
                    else
                    {
                        ViewData["TaskReminder"] = null;
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load tasks in Dashboard");
            }

            return Page();
        }


    }
}