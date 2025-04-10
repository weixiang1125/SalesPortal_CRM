using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedLibrary.Models;
using Task = System.Threading.Tasks.Task;

namespace CRM_Web.Pages
{
    public class UserListModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<Users> Users { get; set; } = new();

        public UserListModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task OnGetAsync()
        {
            Users = await _httpClient.GetFromJsonAsync<List<Users>>("https://localhost:44309/api/User");
        }
    }
}
