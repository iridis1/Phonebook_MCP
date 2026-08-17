using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Phonebook_MCP.Controllers;
using Phonebook_MCP.Data;
using Phonebook_MCP.Services;
using Xunit;

namespace Phonebook_MCP.Tests;

public sealed class PhonebookControllerTests
{
    [Fact]
    public async Task Get_ReturnsBadRequestWhenNameIsMissing()
    {
        await using var database = await CreateDatabaseAsync();
        var controller = new PhonebookController(new PhonebookSearchService(database.Context));

        var result = await controller.Get(null, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Get_ReturnsBadRequestWhenNameIsTooShort()
    {
        await using var database = await CreateDatabaseAsync();
        var controller = new PhonebookController(new PhonebookSearchService(database.Context));

        var result = await controller.Get("S", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Theory]
    [InlineData("%")]
    [InlineData("_")]
    [InlineData("a%z")]
    [InlineData("a_z")]
    public async Task Get_ReturnsBadRequestWhenInvalidCharacter(string invalidChars)
    {
        await using var database = await CreateDatabaseAsync();
        var controller = new PhonebookController(new PhonebookSearchService(database.Context));

        var result = await controller.Get(invalidChars, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Get_ReturnsMatchingContacts()
    {
        await using var database = await CreateDatabaseAsync();
        database.Context.Contacts.Add(new() { Name = "Sabine", Mobile = "06-45678944" });
        await database.Context.SaveChangesAsync();
        var controller = new PhonebookController(new PhonebookSearchService(database.Context));

        var result = await controller.Get("Sabine", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var contacts = Assert.IsAssignableFrom<IReadOnlyList<PhonebookContactResult>>(okResult.Value);
        var contact = Assert.Single(contacts);
        Assert.Equal("Sabine", contact.Name);
        Assert.Equal("06-45678944", contact.Number);
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
