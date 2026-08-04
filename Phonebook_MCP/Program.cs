
namespace Phonebook_MCP;
using Microsoft.EntityFrameworkCore;
using Phonebook_MCP.Data;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Configure database
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=Data/phonebook.db";
        builder.Services.AddDbContext<PhonebookContext>(options => options.UseSqlite(connectionString));

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
