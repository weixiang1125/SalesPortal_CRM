using CRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

[ApiController]
[Route("api/[controller]")]
public class DealController : ControllerBase
{
    private readonly IDealService _dealService;

    public DealController(IDealService dealService)
    {
        _dealService = dealService;
    }

    [Authorize]
    [HttpGet("GetAllDeal")]
    public async Task<IActionResult> GetAll()
    {
        var deals = await _dealService.GetAllDealAsync();
        return Ok(deals);
    }

    [HttpGet("GetDealById/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var deal = await _dealService.GetDealByIdAsync(id);
        if (deal == null) return NotFound();
        return Ok(deal);
    }

    [HttpPost("CreateDeal")]
    public async Task<IActionResult> Create([FromBody] Deal deal)
    {
        var created = await _dealService.CreateDealAsync(deal);
        return CreatedAtAction(nameof(GetById), new { id = created.DealID }, created);
    }

    [HttpPut("UpdateDealById/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Deal deal)
    {
        if (id != deal.DealID)
        {
            // Return a 400 Bad Request if the ID in the URL doesn't match the ID in the body
            return BadRequest("Deal ID mismatch.");
        }

        var success = await _dealService.UpdateDealAsync(deal);

        if (!success)
        {
            // If the update fails (no rows affected), return a 404 Not Found
            return NotFound("Deal not found or update failed.");
        }

        // Return a 204 No Content on successful update
        return NoContent();
    }


    [HttpDelete("DeleteDealById/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _dealService.DeleteDealByIdAsync(id);
        if (!success) return NotFound();

        return NoContent();
    }
}
