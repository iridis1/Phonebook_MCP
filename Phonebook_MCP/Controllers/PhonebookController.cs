using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Phonebook_MCP.Data;

namespace Phonebook_MCP.Controllers;

[ApiController]
[Route("phonebook")]
public class PhonebookController : ControllerBase
{
    private readonly PhonebookContext _context;

    public PhonebookController(PhonebookContext context)
    {
        _context = context;
    }

    // GET /phonebook?name=<NAME>
    [HttpGet]
    public IActionResult Get([FromQuery] string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { error = "query parameter 'name' is required" });
        }

        var pattern = $"%{name}%";

        var results = _context.Contacts
            .Where(c => EF.Functions.Like(c.Name, pattern))
            .Select(c => new
            {
                name = c.Name,
                number = c.Mobile
            })
            .ToList();

        return Ok(results);
    }
}
