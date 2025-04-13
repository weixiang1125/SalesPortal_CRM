using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using SharedLibrary.Models;

namespace CRM_Web.Pages.Contacts
{
    public class ContactModelPage : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public ContactModel ContactModel { get; set; } = new();

        public ContactModelPage(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + "api/Contact/GetAllContacts";

            var response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                ContactModel.Contacts = JsonConvert.DeserializeObject<List<Contact>>(json) ?? new();
            }
            else
            {
                ContactModel.Contacts = new();
            }

            return Page();
        }
    }
}