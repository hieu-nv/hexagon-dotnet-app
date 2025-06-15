using System;
using App.Core.Services.Todo;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace App.Core;

public static class AppCore
{
    public static WebApplicationBuilder UseAppCore(this WebApplicationBuilder builder)
    {
        builder?.Services.AddScoped<TodoService>();
        return builder ?? throw new ArgumentNullException(nameof(builder));
    }
}
