using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using SharedLibrary.DTOs;
using SharedLibrary.Models;
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
                        var taskDetails = TodayOrOverdueTasks.Select(t =>
                        {
                            var isOverdue = t.DueDate?.Date < today;
                            var color = isOverdue ? "text-danger" : "text-dark";
                            var dueDate = t.DueDate?.ToString("dd MMM yyyy") ?? "-";
                            var contact = t.Contact?.Name ?? "-";
                            var deal = t.Deal?.DealName ?? "-"; //  include deal name

                            return $@"
                                <div class='{color}'>
                                    • <strong>{t.TaskName}</strong>: {t.TaskDescription}<br/>
                                    <small>Due: {dueDate} | Contact: {contact} | Deal: {deal}</small>
                                </div>";
                        });

                        ViewData["TaskReminder"] = string.Join("", taskDetails);
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


            var dealResponse = await client.GetAsync("api/deal/GetDealByUserId");
            if (dealResponse.IsSuccessStatusCode && role != "Admin")
            {
                var json = await dealResponse.Content.ReadAsStringAsync();
                var deals = JsonConvert.DeserializeObject<List<Deal>>(json) ?? new();

                var wonDeals = deals.Where(d => d.Stage == "Closed-Won").ToList();
                var lostDeals = deals.Where(d => d.Stage == "Closed-Lost").ToList();


                decimal targetSales = 500_000.00M;
                decimal totalSales = wonDeals.Sum(d => d.Value ?? 0);
                int dealCount = deals.Count;
                int dealWon = wonDeals.Count;
                int dealLost = lostDeals.Count;
                double successRate = dealCount > 0 ? (double)dealWon * 100 / dealCount : 0;
                ViewData["ProgressPercent"] = (int)(totalSales / targetSales * 100);
                ViewData["TargetSales"] = targetSales.ToString("C");
                ViewData["TotalSales"] = totalSales.ToString("C");
                ViewData["DealCount"] = dealCount;
                ViewData["DealWon"] = dealWon;
                ViewData["DealLost"] = dealLost;
                ViewData["SuccessRate"] = $"{successRate:F1}%";
            }

            if (role == "Admin")
            {
                _logger.LogWarning("Calling API: /api/deal/GetDealsGroupedByUser");

                var groupedResponse = await client.GetAsync("api/deal/GetDealsGroupedByUser");
                if (groupedResponse.IsSuccessStatusCode)
                {
                    var groupedJson = await groupedResponse.Content.ReadAsStringAsync();
                    _logger.LogWarning("Grouped JSON: {Json}", groupedJson);
                    var userStats = JsonConvert.DeserializeObject<List<SalesPerformanceDto>>(groupedJson);
                    ViewData["GroupedUserKpis"] = userStats;


                    //  Add this to compute company-wide summary
                    if (userStats == null || userStats.Count == 0)
                    {
                        ViewData["GroupedUserKpis"] = new List<dynamic>();
                        ViewData["TargetSales"] = "RM 0.00";
                        ViewData["TotalSales"] = "RM 0.00";
                        ViewData["DealCount"] = 0;
                        ViewData["DealWon"] = 0;
                        ViewData["DealLost"] = 0;
                        ViewData["SuccessRate"] = "0%";
                    }
                    else
                    {
                        decimal totalSales = 0;
                        int dealCount = 0, won = 0, lost = 0;

                        foreach (var user in userStats)
                        {
                            totalSales += Convert.ToDecimal(user?.TotalSales ?? 0);
                            dealCount += Convert.ToInt32(user?.DealCount ?? 0);
                            won += Convert.ToInt32(user?.WonCount ?? 0);
                            lost += Convert.ToInt32(user?.LostCount ?? 0);
                        }

                        double successRate = dealCount > 0 ? (double)won * 100 / dealCount : 0;
                        int totalUsers = userStats.Count;
                        decimal totalTarget = totalUsers * 500_000M;

                        ViewData["TargetSales"] = totalTarget.ToString("C");
                        ViewData["TotalSales"] = totalSales.ToString("C");
                        ViewData["DealCount"] = dealCount;
                        ViewData["DealWon"] = won;
                        ViewData["DealLost"] = lost;
                        ViewData["SuccessRate"] = $"{successRate:F1}%";
                    }

                }
            }


            return Page();
        }


    }
}