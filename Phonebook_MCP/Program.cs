
namespace Phonebook_MCP;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using Phonebook_MCP.Data;
using Phonebook_MCP.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Configure database
        var connectionString = ResolveSqliteConnectionString(
            builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=../Data/Phonebook.db",
            builder.Environment.ContentRootPath);
        builder.Services.AddDbContext<PhonebookContext>(options => options.UseSqlite(connectionString));
        builder.Services.AddScoped<IPhonebookSearchService, PhonebookSearchService>();

        // Add services to the container.
        builder.Services.AddMcpServer()
            .WithHttpTransport(options =>
            {
                options.Stateless = true;
            })
            .WithToolsFromAssembly();

        builder.Services.AddControllers();
        // OpenAPI - Microsoft.AspNetCore.OpenApi handles Swagger UI automatically
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            // MapOpenApi() provid  es OpenAPI specification and automatic Swagger UI
            app.MapOpenApi();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/openapi/v1.json", "Phonebook API"));
        }

        app.UseAuthorization();

        app.MapMcp("/phonebook");
        app.MapControllers();

        // Initialize the database
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PhonebookContext>();
            dbContext.Database.EnsureCreated();
        }

        app.Run();
    }

    private static string ResolveSqliteConnectionString(string connectionString, string contentRootPath)
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder(connectionString);
        var dataSource = connectionStringBuilder.DataSource;

        if (!string.IsNullOrWhiteSpace(dataSource)
            && dataSource != ":memory:"
            && !Path.IsPathRooted(dataSource))
        {
            var databasePath = Path.GetFullPath(Path.Combine(contentRootPath, dataSource));
            var databaseDirectory = Path.GetDirectoryName(databasePath);

            if (!string.IsNullOrEmpty(databaseDirectory))
            {
                Directory.CreateDirectory(databaseDirectory);
            }

            connectionStringBuilder.DataSource = databasePath;
        }

        return connectionStringBuilder.ConnectionString;
    }
}
