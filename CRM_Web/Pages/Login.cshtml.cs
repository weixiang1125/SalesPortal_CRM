//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;

//public class LoginModel : PageModel
//{
//    private readonly IHttpClientFactory _clientFactory;

//    public LoginModel(IHttpClientFactory clientFactory)
//    {
//        _clientFactory = clientFactory;
//    }

//    [BindProperty]
//    public LoginRequest LoginInput { get; set; }

//    public async Task<IActionResult> OnPostAsync()
//    {
//        var client = _clientFactory.CreateClient("CRM_API");
//        var response = await client.PostAsJsonAsync("api/login", LoginInput);

//        if (response.IsSuccessStatusCode)
//        {
//            // Get the token from the response
//            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

//            // Ensure token exists
//            if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Token))
//            {
//                // Store the token in session (or localStorage in the frontend)
//                HttpContext.Session.SetString("AuthToken", tokenResponse.Token);

//                // You can also store additional user info, if needed
//                HttpContext.Session.SetString("Username", LoginInput.Username);

//                // Redirect to the main page or dashboard
//                return RedirectToPage("/Index");
//            }
//            else
//            {
//                ModelState.AddModelError(string.Empty, "Invalid login response");
//            }
//        }
//        else
//        {
//            ModelState.AddModelError(string.Empty, "Invalid username or password");
//        }

//        return Page();
//    }
//}

//// Response model to deserialize token from API response
//public class TokenResponse
//{
//    public string Token { get; set; }
//}
