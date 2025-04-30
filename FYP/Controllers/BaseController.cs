using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM_API.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        private readonly ILogger<BaseController> _logger;

        // Inject logger
        public BaseController(ILogger<BaseController> logger)
        {
            _logger = logger;
        }

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
                _logger.LogInformation("User roles: {Roles}", string.Join(",", User.Claims
                    .Where(c => c.Type == ClaimTypes.Role || c.Type.Contains("role"))
                    .Select(c => $"{c.Type}:{c.Value}")));

                return User.IsInRole("Admin");
            }
        }

    }
}
