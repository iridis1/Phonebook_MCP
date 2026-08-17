using Microsoft.EntityFrameworkCore;
using Phonebook_MCP.Data;

namespace Phonebook_MCP.Services;

public sealed record PhonebookSearchRequest(string Name);

public sealed record PhonebookContactResult(string Name, string Number);

public sealed record PhonebookSearchResult(string Query, int Count, IReadOnlyList<PhonebookContactResult> Results);

public sealed class PhonebookSearchService : IPhonebookSearchService
{
    public const int MinimumSearchNameLength = 2;

    private readonly PhonebookContext _context;

    public PhonebookSearchService(PhonebookContext context)
    {
        _context = context;
    }

    public async Task<PhonebookSearchResult> SearchAsync(string name, CancellationToken cancellationToken = default)
    {
        var query = name.Trim();

        if (!IsValidSearchName(query))
        {
            throw new ArgumentException($"Name must be at least {MinimumSearchNameLength} characters long and not contain disallowed characters.", nameof(name));
        }

        var pattern = $"%{query}%";

        var results = await _context.Contacts
            .Where(contact => EF.Functions.Like(contact.Name, pattern))
            .OrderBy(contact => contact.Name)
            .Select(contact => new PhonebookContactResult(contact.Name, contact.Mobile))
            .ToListAsync(cancellationToken);

        return new PhonebookSearchResult(query, results.Count, results);
    }

    public static bool IsValidSearchName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.Trim().Length >= MinimumSearchNameLength && !name.Contains('%') && !name.Contains('_');
    }
}
