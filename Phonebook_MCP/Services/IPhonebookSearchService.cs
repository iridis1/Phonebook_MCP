namespace Phonebook_MCP.Services;

public interface IPhonebookSearchService
{
    Task<PhonebookSearchResult> SearchAsync(string name, CancellationToken cancellationToken = default);
}
