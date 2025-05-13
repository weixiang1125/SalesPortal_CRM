using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using SharedLibrary.DTOs;
using System.Net;
using System.Net.Http.Headers;


namespace CRM_Web.Pages.Contacts
{
    public class ContactModelPage : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public ContactDtoModel ContactModel { get; set; } = new();


        public ContactModelPage(IHttpClientFactory httpClientFactory, IConfiguration configuration)
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
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + "api/Contact/GetContactsByUserId";  // Adjusted to use user-specific endpoint

            // 3. Add authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // 4. Make API call
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    ContactModel.Contacts = JsonConvert.DeserializeObject<List<ContactDto>>(jsonString) ?? new List<ContactDto>();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Token expired or invalid - redirect to login
                    return RedirectToPage("/Login");
                }
                else
                {
                    // Handle other errors
                    ContactModel.Contacts = new List<ContactDto>();
                    // Consider logging the error or showing a user-friendly message
                }
            }
            catch (Exception ex)
            {
                // Handle network errors
                ContactModel.Contacts = new List<ContactDto>();
                // Consider logging the exception for debugging purposes
            }

            return Page();
        }

        public class ContactDtoModel
        {
            public List<ContactDto> Contacts { get; set; } = new();
        }
    }
}
