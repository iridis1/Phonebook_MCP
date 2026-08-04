var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Phonebook_MCP>("phonebook-mcp");

builder.Build().Run();
