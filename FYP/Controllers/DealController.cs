using CRM_API.Services;
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
        if (id != deal.DealID) return BadRequest();

        var success = await _dealService.UpdateDealAsync(deal);
        if (!success) return NotFound();

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
