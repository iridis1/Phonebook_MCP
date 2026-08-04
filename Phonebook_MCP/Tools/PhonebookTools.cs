using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using Phonebook_MCP.Data;

namespace Phonebook_MCP.Tools;

[McpServerToolType]
public sealed class PhonebookTools
{
    private readonly PhonebookContext _context;

    public PhonebookTools(PhonebookContext context)
    {
        _context = context;
    }

    [McpServerTool]
    [Description("Search the phonebook by contact name and return matching mobile numbers.")]
    public async Task<object> SearchPhonebook(
        [Description("The contact name, or part of the contact name, to search for.")]
        string name,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new
            {
                error = "The 'name' argument is required."
            };
        }

        var pattern = $"%{name.Trim()}%";

        var results = await _context.Contacts
            .Where(contact => EF.Functions.Like(contact.Name, pattern))
            .OrderBy(contact => contact.Name)
            .Select(contact => new
            {
                name = contact.Name,
                number = contact.Mobile
            })
            .ToListAsync(cancellationToken);

        return new
        {
            query = name,
            count = results.Count,
            results
        };
    }
}
