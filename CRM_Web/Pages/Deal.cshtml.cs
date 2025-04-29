using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using SharedLibrary.Models;
using System.Net;
using System.Net.Http.Headers;

namespace CRM_Web.Pages.Deals
{
    public class DealModelPage : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public DealModel DealModel { get; set; } = new();

        public DealModelPage(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // 1. Get token from HTTP-only cookie
            var token = Request.Cookies["authToken"];

            if (string.IsNullOrEmpty(token))
            {
                // Token not found - redirect to login
                return RedirectToPage("/Login");
            }

            // 2. Create HTTP client and set up request
            var client = _httpClientFactory.CreateClient();
            string apiUrl = _configuration["ApiSettings:BaseUrl"] + "api/Deal/GetDealByUserId";

            // 3. Add authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // 4. Make API call
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    DealModel.Deals = JsonConvert.DeserializeObject<List<Deal>>(jsonString) ?? new List<Deal>();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Token expired or invalid - redirect to login
                    return RedirectToPage("/Login");
                }
                else
                {
                    // Handle other errors
                    DealModel.Deals = new List<Deal>();
                    // Consider logging the error

                }
            }
            catch (Exception ex)
            {
                // Handle network errors
                DealModel.Deals = new List<Deal>();

            }

            return Page();
        }





    }
}
