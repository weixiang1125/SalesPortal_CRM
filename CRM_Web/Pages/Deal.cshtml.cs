using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using SharedLibrary.Models;

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
            var client = _httpClientFactory.CreateClient();
            string apiUrl = _configuration["ApiSettings:BaseUrl"] + "api/Deal/GetAllDeal";

            var response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                DealModel.Deals = JsonConvert.DeserializeObject<List<Deal>>(jsonString) ?? new();
            }
            else
            {
                // Log or handle error
                DealModel.Deals = new();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page(); // Return with validation errors
            }

            var client = _httpClientFactory.CreateClient();
            string apiUrl = _configuration["ApiSettings:BaseUrl"] + "api/Deal/CreateDeal";

            var jsonContent = new StringContent(JsonConvert.SerializeObject(DealModel.NewDeal), System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiUrl, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage(); // Refresh to show the new deal
            }

            // Optional: Add error handling/logging here
            return Page();
        }

        // Handler for editing a deal
        public async Task<IActionResult> OnPostEditAsync()
        {
            var client = _httpClientFactory.CreateClient();

            // Assuming DealModel.DealToEdit.DealID contains the correct ID
            string apiUrl = _configuration["ApiSettings:BaseUrl"] + "api/Deal/UpdateDealById/" + DealModel.DealToEdit.DealID;

            var response = await client.PutAsJsonAsync(apiUrl, DealModel.DealToEdit);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage(); // Reload the page to show the updated deal
            }
            else
            {
                // Handle the error properly (e.g., display an error message)
                ModelState.AddModelError(string.Empty, "Failed to update deal.");
                return Page();
            }
        }




    }
}
