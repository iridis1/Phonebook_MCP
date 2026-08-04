using Microsoft.EntityFrameworkCore;
using Phonebook_MCP.Models;

namespace Phonebook_MCP.Data;

public class PhonebookContext : DbContext
{
    public PhonebookContext(DbContextOptions<PhonebookContext> options) : base(options)
    {
    }

    public DbSet<Contact> Contacts { get; set; } = null!;
}
