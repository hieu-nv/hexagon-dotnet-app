using System;
using App.Core.Services.Todo;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace App.Core;

public static class Core
{
    public static WebApplicationBuilder UseCore(this WebApplicationBuilder builder)
    {
        builder?.Services.AddScoped<TodoService>();
        return builder ?? throw new ArgumentNullException(nameof(builder));
    }
}
