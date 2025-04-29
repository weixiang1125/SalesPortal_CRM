using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM_API.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        // Property to get Current UserId from the JWT token
        protected int CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            }
        }

        // Property to get Current Username
        protected string CurrentUsername
        {
            get
            {
                return User.Identity?.Name ?? string.Empty;
            }
        }

        // Property to check if user is Admin
        protected bool IsAdmin
        {
            get
            {
                return User.IsInRole("Admin");
            }
        }
    }
}
