using CRM_API.Controllers;
using CRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

[ApiController]
[Route("api/[controller]")]
public class ContactController : BaseController
{
    private readonly IContactService _contactService;
    private readonly ILogger<ContactController> _logger;

    public ContactController(IContactService contactService, ILogger<ContactController> logger)
        : base(logger)  // Pass the logger to the BaseController constructor
    {
        _contactService = contactService;
        _logger = logger;
    }

    [Authorize]
    [HttpGet("GetContactsByUserId")]
    public async Task<ActionResult<IEnumerable<Contact>>> GetContactsByUserId()
    {
        _logger.LogInformation($"CurrentUserId: {CurrentUserId}, IsAdmin: {IsAdmin}");
        if (IsAdmin)
        {
            var allContacts = await _contactService.GetAllContactsAsync();
            return Ok(allContacts);
        }
        else
        {
            var userContacts = await _contactService.GetContactsByUserIdAsync(CurrentUserId);
            return Ok(userContacts);
        }
    }



    [Authorize]
    [HttpGet("GetAllContacts")]
    public async Task<IActionResult> GetAllContacts()
    {
        var contacts = await _contactService.GetAllContactsAsync();
        return Ok(contacts);
    }
    [Authorize]
    [HttpGet("GetContactById/{id}")]
    public async Task<IActionResult> GetContactById(int id)
    {
        var contact = await _contactService.GetContactByIdAsync(id);
        if (contact == null) return NotFound();
        return Ok(contact);
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
