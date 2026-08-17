using Microsoft.AspNetCore.Mvc;
using Phonebook_MCP.Controllers;
using Phonebook_MCP.Services;
using Xunit;

namespace Phonebook_MCP.Tests;

public sealed class PhonebookControllerTests
{
    [Fact]
    public async Task Get_ReturnsBadRequestWhenNameIsMissing()
    {
        var searchService = new FakePhonebookSearchService();
        var controller = new PhonebookController(searchService);

        var result = await controller.Get(null, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, searchService.SearchCallCount);
    }

    [Fact]
    public async Task Get_ReturnsBadRequestWhenNameIsTooShort()
    {
        var searchService = new FakePhonebookSearchService();
        var controller = new PhonebookController(searchService);

        var result = await controller.Get("S", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, searchService.SearchCallCount);
    }

    [Theory]
    [InlineData("%")]
    [InlineData("_")]
    [InlineData("a%z")]
    [InlineData("a_z")]
    public async Task Get_ReturnsBadRequestWhenInvalidCharacter(string invalidChars)
    {
        var searchService = new FakePhonebookSearchService();
        var controller = new PhonebookController(searchService);

        var result = await controller.Get(invalidChars, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, searchService.SearchCallCount);
    }

    [Fact]
    public async Task Get_ReturnsMatchingContacts()
    {
        var expectedResults = new[]
        {
            new PhonebookContactResult("Sabine", "06-45678944")
        };
        var searchService = new FakePhonebookSearchService(new PhonebookSearchResult("Sabine", 1, expectedResults));
        var controller = new PhonebookController(searchService);

        var result = await controller.Get("Sabine", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var contacts = Assert.IsAssignableFrom<IReadOnlyList<PhonebookContactResult>>(okResult.Value);
        var contact = Assert.Single(contacts);
        Assert.Equal("Sabine", contact.Name);
        Assert.Equal("06-45678944", contact.Number);
        Assert.Equal("Sabine", searchService.LastSearchName);
        Assert.Equal(1, searchService.SearchCallCount);
    }

    private sealed class FakePhonebookSearchService : IPhonebookSearchService
    {
        private readonly PhonebookSearchResult _result;

        public FakePhonebookSearchService(PhonebookSearchResult? result = null)
        {
            _result = result ?? new PhonebookSearchResult("", 0, []);
        }

        public string? LastSearchName { get; private set; }

        public int SearchCallCount { get; private set; }

        public Task<PhonebookSearchResult> SearchAsync(string name, CancellationToken cancellationToken = default)
        {
            LastSearchName = name;
            SearchCallCount++;
            return Task.FromResult(_result);
        }
    }
}
