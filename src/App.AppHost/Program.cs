using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add the API project - SQLite connection is managed within App.Data layer
// Path is relative to the AppHost project directory
builder.AddProject("api", "../App.Api/App.Api.csproj").WithExternalHttpEndpoints();

builder.Build().Run();
