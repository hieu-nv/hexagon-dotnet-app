WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.UseAppCore();
builder.UseAppData();
builder.UseAppGateway();

builder.UseTodo();
builder.UsePokemon();

WebApplication app = builder.Build();
app.UseAppData();

app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    _ = app.UseDeveloperExceptionPage();
}

app.UseTodo();
app.UsePokemon();

await app.RunAsync().ConfigureAwait(false);
