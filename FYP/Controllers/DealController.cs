using AutoMapper;
using CRM_API.Controllers;
using CRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.DTOs;
using SharedLibrary.Models;
using SharedLibrary.Utils;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class DealController : BaseController
{
    private readonly IDealService _dealService;
    private readonly IUsersService _usersService;
    private readonly IMapper _mapper;

    public DealController(IDealService dealService, IUsersService usersService, ILogger<BaseController> logger, IMapper mapper)
        : base(logger)  // Pass the logger to the BaseController constructor
    {
        _dealService = dealService;
        _usersService = usersService;
        _mapper = mapper;
    }

    [Authorize]
    [HttpGet("GetDealByUserId")]
    public async Task<ActionResult<IEnumerable<DealDto>>> GetDealByUserId()
    {
        var deals = IsAdmin
            ? await _dealService.GetAllDealAsync()
            : await _dealService.GetDealsByUserIdAsync(CurrentUserId);

        var result = _mapper.Map<IEnumerable<DealDto>>(deals);
        return Ok(result);
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
    public async Task<ActionResult<DealDto>> GetDealById(int id)
    {
        var deal = await _dealService.GetDealByIdAsync(id);
        if (deal == null) return NotFound();

        var dto = _mapper.Map<DealDto>(deal);
        return Ok(dto);
    }

    [Authorize]
    [HttpPost("CreateDeal")]
    public async Task<IActionResult> Create([FromBody] Deal deal)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        deal.CreatedDate = TimeHelper.Now();
        deal.CreatedBy = userId;

        var created = await _dealService.CreateDealAsync(deal, userId);
        return CreatedAtAction(nameof(GetDealById), new { id = created.DealID }, created);
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
