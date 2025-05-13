using AutoMapper;
using CRM_API.Controllers;
using CRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.DTOs;
using SharedLibrary.Models;

[ApiController]
[Route("api/[controller]")]
public class ContactController : BaseController
{
    private readonly IContactService _contactService;
    private readonly ILogger<ContactController> _logger;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;

    public ContactController(IContactService contactService, ILogger<ContactController> logger, IMapper mapper, ApplicationDbContext dbContext)
        : base(logger)  // Pass the logger to the BaseController constructor
    {
        _contactService = contactService;
        _logger = logger;
        _mapper = mapper;
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpGet("GetContactsByUserId")]
    public async Task<ActionResult<IEnumerable<ContactDto>>> GetContactsByUserId()
    {
        IEnumerable<Contact> contacts;

        if (IsAdmin)
        {
            contacts = await _contactService.GetAllContactsWithUsersAsync();
        }
        else
        {
            contacts = await _contactService.GetContactsByUserIdAsync(CurrentUserId);
        }

        var result = _mapper.Map<IEnumerable<ContactDto>>(contacts);
        return Ok(result);
    }




    [Authorize]
    [HttpGet("GetAllContacts")]
    public async Task<IActionResult> GetAllContacts()
    {
        var contacts = await _contactService.GetAllContactsWithUsersAsync();

        var result = contacts.Select(c => new ContactDto
        {
            ContactID = c.ContactID,
            Name = c.Name,
            Email = c.Email,
            Phone = c.Phone,
            Company = c.Company,
            Notes = c.Notes,
            Status = c.Status,
            CreatedDate = c.CreatedDate,
            CreatedByUsername = c.CreatedByUser?.Username,
            UpdatedDate = c.UpdatedDate,
            UpdatedByUsername = c.UpdatedByUser?.Username
        });

        return Ok(result);
    }
    [Authorize]
    [HttpGet("GetContactById/{id}")]
    public async Task<IActionResult> GetContactById(int id)
    {
        var contact = await _dbContext.DBContacts
            .Include(c => c.CreatedByUser)
            .Include(c => c.UpdatedByUser)
            .FirstOrDefaultAsync(c => c.ContactID == id);

        if (contact == null) return NotFound();

        var dto = _mapper.Map<ContactDto>(contact);
        return Ok(dto);
    }


    [Authorize]
    [HttpPost("CreateContact")]
    public async Task<IActionResult> CreateContact([FromBody] Contact contact)
    {
        var userId = CurrentUserId;  // Get the userId from the JWT token
        var created = await _contactService.CreateContactAsync(contact, userId);
        return CreatedAtAction(nameof(GetContactById), new { id = created.ContactID }, created);
    }

    [Authorize]
    [HttpPut("UpdateContactById/{id}")]
    public async Task<IActionResult> UpdateContactById(int id, [FromBody] Contact contact)
    {
        if (id != contact.ContactID) return BadRequest();

        var userId = CurrentUserId;  // Get the userId from the JWT token
        var success = await _contactService.UpdateContactAsync(contact, userId);
        if (!success) return NotFound();

        return NoContent();
    }
    [Authorize]
    [HttpDelete("DeleteContactById/{id}")]
    public async Task<IActionResult> DeleteContactById(int id)
    {
        var success = await _contactService.DeleteContactByIdAsync(id);
        if (!success) return NotFound();

        return NoContent();
    }
}
