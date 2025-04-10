using CRM_API.Services;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly IContactService _contactService;

    public ContactController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpGet("GetAllContacts")]
    public async Task<IActionResult> GetAllContacts()
    {
        var contacts = await _contactService.GetAllContactsAsync();
        return Ok(contacts);
    }

    [HttpGet("GetContactById/{id}")]
    public async Task<IActionResult> GetContactById(int id)
    {
        var contact = await _contactService.GetContactByIdAsync(id);
        if (contact == null) return NotFound();
        return Ok(contact);
    }

    [HttpPost("CreateContact")]
    public async Task<IActionResult> CreateContact([FromBody] Contact contact)
    {
        var created = await _contactService.CreateContactAsync(contact);
        return CreatedAtAction(nameof(GetContactById), new { id = created.ContactID }, created);
    }

    [HttpPut("UpdateContactById/{id}")]
    public async Task<IActionResult> UpdateContactById(int id, [FromBody] Contact contact)
    {
        if (id != contact.ContactID) return BadRequest();

        var success = await _contactService.UpdateContactAsync(contact);
        if (!success) return NotFound();

        return NoContent();
    }

    [HttpDelete("DeleteContactById/{id}")]
    public async Task<IActionResult> DeleteContactById(int id)
    {
        var success = await _contactService.DeleteContactByIdAsync(id);
        if (!success) return NotFound();

        return NoContent();
    }
}
