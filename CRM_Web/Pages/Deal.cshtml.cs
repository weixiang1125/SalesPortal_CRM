using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM_Web.Pages.Deals
{
    public class DealModelPage : PageModel
    {
        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Login");

            ViewData["Username"] = username;
            ViewData["Role"] = role;

            return Page();
        }
    }
}
