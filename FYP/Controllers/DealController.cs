using CRM_API.Controllers;
using CRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class DealController : BaseController
{
    private readonly IDealService _dealService;
    private readonly IUsersService _usersService;

    public DealController(IDealService dealService, IUsersService usersService, ILogger<BaseController> logger)
        : base(logger)  // Pass the logger to the BaseController constructor
    {
        _dealService = dealService;
        _usersService = usersService;
    }

    [Authorize]
    [HttpGet("GetDealByUserId")]
    public async Task<ActionResult<IEnumerable<Deal>>> GetDealByUserId()
    {
        if (IsAdmin)
        {
            var allDeals = await _dealService.GetAllDealAsync();
            return Ok(allDeals);
        }
        else
        {
            var userDeals = await _dealService.GetDealsByUserIdAsync(CurrentUserId);
            return Ok(userDeals);
        }
    }


    [Authorize]
    [HttpGet("GetAllDeal")]
    public async Task<IActionResult> GetAll()
    {
        var deals = await _dealService.GetAllDealAsync();
        return Ok(deals);
    }

    [Authorize]
    [HttpGet("GetDealById/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var deal = await _dealService.GetDealByIdAsync(id);
        if (deal == null) return NotFound();
        return Ok(deal);
    }

    [Authorize]
    [HttpPost("CreateDeal")]
    public async Task<IActionResult> Create([FromBody] Deal deal)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        deal.CreatedDate = DateTime.UtcNow;
        deal.CreatedBy = userId;

        var created = await _dealService.CreateDealAsync(deal, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.DealID }, created);
    }

    [Authorize]
    [HttpPut("UpdateDealById/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Deal deal)
    {
        if (id != deal.DealID)
        {
            // Return a 400 Bad Request if the ID in the URL doesn't match the ID in the body
            return BadRequest("Deal ID mismatch.");
        }
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var success = await _dealService.UpdateDealAsync(deal, userId);

        if (!success)
        {
            // If the update fails (no rows affected), return a 404 Not Found
            return NotFound("Deal not found or update failed.");
        }

        // Return a 204 No Content on successful update
        return NoContent();
    }

    [Authorize]
    [HttpDelete("DeleteDealById/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _dealService.DeleteDealByIdAsync(id);
        if (!success) return NotFound();

        return NoContent();
    }
}
