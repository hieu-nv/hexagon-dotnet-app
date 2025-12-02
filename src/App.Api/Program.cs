WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.UseAppCore();
builder.UseAppData();

builder.UseTodo();

WebApplication app = builder.Build();
app.UseAppData();

if (app.Environment.IsDevelopment())
{
    _ = app.UseDeveloperExceptionPage();
}

app.UseTodo();

await app.RunAsync().ConfigureAwait(false);
