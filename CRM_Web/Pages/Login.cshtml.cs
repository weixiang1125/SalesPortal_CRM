using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM_Web.Pages
{
    [IgnoreAntiforgeryToken]
    public class LoginModel : PageModel
    {
        public void OnGet()
        {
            // Optional: clear session on load
            HttpContext.Session.Clear();
        }


        public IActionResult OnPostSetSession([FromBody] SessionUser user)
        {
            if (user == null || user.UserID == 0)
                return BadRequest("Invalid session data");
            Console.WriteLine("SetSession called: " + user.UserID);
            HttpContext.Session.SetString("UserID", user.UserID.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            return new JsonResult(new { success = true });
        }
        public IActionResult OnPostSetToken([FromBody] string token)
        {
            Response.Cookies.Append("authToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            return new JsonResult(new { success = true });
        }


        public class SessionUser
        {
            public int UserID { get; set; }
            public string Username { get; set; }
            public string Role { get; set; }
        }
    }
}
