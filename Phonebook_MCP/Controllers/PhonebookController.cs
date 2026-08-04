using Microsoft.AspNetCore.Mvc;
using Phonebook_MCP.Services;

namespace Phonebook_MCP.Controllers;

[ApiController]
[Route("phonebook")]
public class PhonebookController : ControllerBase
{
    private readonly PhonebookSearchService _phonebookSearch;

    public PhonebookController(PhonebookSearchService phonebookSearch)
    {
        _phonebookSearch = phonebookSearch;
    }

    // GET /phonebook?name=<NAME>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PhonebookContactResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get([FromQuery] string? name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { error = "query parameter 'name' is required" });
        }

        var result = await _phonebookSearch.SearchAsync(name, cancellationToken);

        return Ok(result.Results);
    }

    // POST /phonebook/search
    [HttpPost("search")]
    [ProducesResponseType(typeof(PhonebookSearchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search([FromBody] PhonebookSearchRequest? request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Name))
        {
            return BadRequest(new { error = "body property 'name' is required" });
        }

        var result = await _phonebookSearch.SearchAsync(request.Name, cancellationToken);

        return Ok(result);
    }
}
