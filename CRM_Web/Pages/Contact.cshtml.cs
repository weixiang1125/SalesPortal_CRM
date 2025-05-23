using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedLibrary.DTOs;


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

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Login");

            ViewData["Username"] = username;
            ViewData["Role"] = role;

            return Page(); // No API call here anymore
        }



        public class ContactDtoModel
        {
            public List<ContactDto> Contacts { get; set; } = new();
        }
    }
}

