using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Phonebook_MCP.Data;
using Phonebook_MCP.Models;
using Phonebook_MCP.Services;
using Xunit;

namespace Phonebook_MCP.Tests;

public sealed class PhonebookSearchServiceTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("S")]
    [InlineData(" S ")]
    [InlineData("%a")]
    [InlineData("a_")]
    public void IsValidSearchName_RejectsInvalidSearchTerms(string? name)
    {
        Assert.False(PhonebookSearchService.IsValidSearchName(name));
    }

    [Theory]
    [InlineData("Sa")]
    [InlineData(" Sabine ")]
    public void IsValidSearchName_AcceptsValidSearchTerms(string name)
    {
        Assert.True(PhonebookSearchService.IsValidSearchName(name));
    }

    [Fact]
    public async Task SearchAsync_ReturnsMatchesOrderedByName()
    {
        await using var database = await CreateDatabaseAsync();
        var service = new PhonebookSearchService(database.Context);

        var result = await service.SearchAsync("Sa");

        Assert.Equal("Sa", result.Query);
        Assert.Equal(2, result.Count);
        Assert.Equal(
            ["Sabine", "Sarah"],
            result.Results.Select(contact => contact.Name).ToArray());
    }

    [Fact]
    public async Task SearchAsync_TrimsSearchTerm()
    {
        await using var database = await CreateDatabaseAsync();
        var service = new PhonebookSearchService(database.Context);

        var result = await service.SearchAsync(" Sabine ");

        Assert.Equal("Sabine", result.Query);
        var contact = Assert.Single(result.Results);
        Assert.Equal("06-45678922", contact.Number);
    }

    [Fact]
    public async Task SearchAsync_ThrowsForTooShortSearchTerm()
    {
        await using var database = await CreateDatabaseAsync();
        var service = new PhonebookSearchService(database.Context);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.SearchAsync("S"));

        Assert.Equal("name", exception.ParamName);
    }

    private static async Task<TestDatabase> CreateDatabaseAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PhonebookContext>()
            .UseSqlite(connection)
            .Options;

        var context = new PhonebookContext(options);
        await context.Database.EnsureCreatedAsync();

        context.Contacts.AddRange(
            new Contact { Name = "Sabine", Mobile = "06-45678922" },
            new Contact { Name = "Sarah", Mobile = "06-33333333" },
            new Contact { Name = "John", Mobile = "06-65454121" },
            new Contact { Name = "Adam", Mobile = "06-11111111" },
            new Contact { Name = "Zara", Mobile = "06-22222222" });
        await context.SaveChangesAsync();

        return new TestDatabase(connection, context);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public TestDatabase(SqliteConnection connection, PhonebookContext context)
        {
            _connection = connection;
            Context = context;
        }

        public PhonebookContext Context { get; }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
