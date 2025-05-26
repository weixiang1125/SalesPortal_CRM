using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using SharedLibrary.Models;
using System.Net.Http.Headers;

namespace CRM_Web.Pages.Profile
{
    public class ProfileModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;

        public ProfileModel(IHttpClientFactory factory, IConfiguration config)
        {
            _factory = factory;
            _config = config;
        }

        public Users User { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId)) return RedirectToPage("/Login");

            var client = _factory.CreateClient();
            client.BaseAddress = new Uri(_config["ApiSettings:BaseUrl"]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["authToken"]);

            var res = await client.GetAsync($"api/Users/GetUsersById/{userId}");
            if (!res.IsSuccessStatusCode) return RedirectToPage("/Login");

            var json = await res.Content.ReadAsStringAsync();
            User = JsonConvert.DeserializeObject<Users>(json)!;

            return Page();
        }
    }
}
