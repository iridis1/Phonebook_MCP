using System.ComponentModel;
using ModelContextProtocol.Server;
using Phonebook_MCP.Services;

namespace Phonebook_MCP.Tools;

[McpServerToolType]
public sealed class PhonebookTools
{
    private readonly PhonebookSearchService _phonebookSearch;

    public PhonebookTools(PhonebookSearchService phonebookSearch)
    {
        _phonebookSearch = phonebookSearch;
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

        if (!PhonebookSearchService.IsValidSearchName(name))
        {
            return new
            {
                error = $"The 'name' argument must be at least {PhonebookSearchService.MinimumSearchNameLength} characters long and not contain disallowed characters."
            };
        }

        return await _phonebookSearch.SearchAsync(name, cancellationToken);
    }
}
